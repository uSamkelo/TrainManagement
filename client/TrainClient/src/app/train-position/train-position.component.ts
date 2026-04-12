import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TrainPositionService } from '../services/train-position.service';
import { SignalRService } from '../services/signal-r.service';
import * as L from 'leaflet';
import { HttpClientModule } from '@angular/common/http';
import { IPosition } from '../interfaces/IPositions';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-train-position',
  templateUrl: './train-position.component.html',
  standalone: true,
  imports: [HttpClientModule, CommonModule],
  styleUrls: ['./train-position.component.css'],
})
export class TrainPositionComponent implements OnInit, OnDestroy {
  positions: IPosition [] = [];
  map!: L.Map;
  private markerLayer = L.layerGroup();
  private destroy$ = new Subject<void>();

  constructor(
    private trainPositionService: TrainPositionService,
    private signalRService: SignalRService
  ) {}

  // Create custom train icons based on status
  private createTrainIcon(status: string) {
    const statusClass = status.toLowerCase() || 'default';

    return L.divIcon({
      html: `
        <div class="train-icon-wrapper ${statusClass}">🚂</div>
      `,
      iconSize: [30, 30],
      className: 'train-icon'
    });
  }

  ngOnInit(): void {
    console.log('ngOnInit started');
    
    // Load initial train positions
    this.trainPositionService.getTrainPositions().subscribe((positions: IPosition[]) => {
      console.log('Received positions:', positions);
      console.log('Positions length:', positions.length);
      console.log('First position:', positions[0]);
      this.positions = positions;
      console.log('this.positions assigned:', this.positions);
      console.log('this.positions after assignment:', this.positions);
      this.updateMap();
    });

    // Initialize the map
    this.map = L.map('map').setView([-33.9212, 18.4970], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
    }).addTo(this.map);
    this.markerLayer.addTo(this.map);

    // Connect to SignalR hub and subscribe to real-time updates
    this.signalRService.start()
      .then(() => {
        console.log('SignalR connected');
        // Subscribe to train updates
        this.signalRService.trainUpdate$
          .pipe(takeUntil(this.destroy$))
          .subscribe((update) => {
            console.log('Real-time update received:', update);
            this.updateTrainPosition(update);
            this.updateMap();
          });
      })
      .catch((error) => {
        console.error('Failed to connect to SignalR:', error);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.signalRService.stop().catch((error) => console.error('Error stopping SignalR:', error));
  }

  private updateTrainPosition(update: any) {
    const index = this.positions.findIndex(p => p.trainId === update.trainId);
    if (index > -1) {
      this.positions[index] = {
        trainId: update.trainId,
        latitude: update.latitude,
        longitude: update.longitude,
        status: update.status,
        eta: update.eta
      };
    }
  }

  updateMap() {
    this.markerLayer.clearLayers();

    this.positions.forEach((position, index) => {
      // Double-check for 0,0 or nulls if your C# defaults to 0
      if (position.latitude && position.longitude) {
        const marker = L.marker([position.latitude, position.longitude], {
          icon: this.createTrainIcon(position.status)
        });

        marker.bindPopup(`
          <div style="font-family: Arial; width: 200px;">
            <h4 style="margin: 5px 0; color: #2c3e50;">${position.trainId}</h4>
            <hr style="margin: 5px 0; border: none; border-top: 1px solid #ecf0f1;">
            <p style="margin: 3px 0;"><strong>Status:</strong> <span style="color: ${position.status.toLowerCase() === 'delayed' ? '#e74c3c' : '#27ae60'};">${position.status}</span></p>
            <p style="margin: 3px 0;"><strong>Location:</strong> ${position.latitude.toFixed(4)}, ${position.longitude.toFixed(4)}</p>
            <p style="margin: 3px 0;"><strong>ETA:</strong> ${position.eta}</p>
          </div>
        `);

        // Add to the layer group instead of the map directly
        marker.addTo(this.markerLayer);
      }
    });

    // Fit map bounds to show all markers
    if (this.positions.length > 0) {
      const group = new L.FeatureGroup(Array.from(this.markerLayer.getLayers()));
      this.map.fitBounds(group.getBounds().pad(0.1));
    }
  }
}
