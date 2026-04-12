import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { TrainPositionService } from '../services/train-position.service';
import { SignalRService } from '../services/signal-r.service';
import * as L from 'leaflet';
import { IPosition, IRoute, IStation, INextArrivalResult } from '../interfaces/IPositions';
import { Subject, Observable } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-train-position',
  templateUrl: './train-position.component.html',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styleUrls: ['./train-position.component.css'],
})
export class TrainPositionComponent implements OnInit, OnDestroy {
  positions: IPosition[] = [];
  routes: Map<string, IRoute> = new Map();
  map!: L.Map;
  private markerLayer = L.layerGroup();
  private trailLayer = L.layerGroup();
  private routeLayer = L.layerGroup();
  private stationLayer = L.layerGroup();
  private destroy$ = new Subject<void>();
  
  // Loading and error states
  isLoading: boolean = true;
  loadError: string | null = null;

  // Station and location selection
  selectedStation: string = '';
  selectedStationId: number | null = null;
  latitude: number | null = null;
  longitude: number | null = null;
  stations: IStation[] = [];
  isLoadingLocations: boolean = false;
  hasLocationPermission: boolean = false;
  nextArrivalResult: INextArrivalResult | null = null;

  private userLocationMarker: L.Marker | null = null;

  private readonly API_BASE_URL = 'http://localhost:5176';

  constructor(
    private trainPositionService: TrainPositionService,
    private signalRService: SignalRService,
    private http: HttpClient,
  ) {}

  // Create custom train icons based on status
  private createTrainIcon(status: string) {
    const statusClass = status.toLowerCase() || 'default';

    return L.divIcon({
      html: `
        <div class="train-icon-wrapper ${statusClass}">🚂</div>
      `,
      iconSize: [30, 30],
      className: 'train-icon',
    });
  }

  ngOnInit(): void {
    console.log('ngOnInit started');

    // Initialize the map first - center on Cape Town
    this.map = L.map('map').setView([-33.925, 18.5], 12);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
    }).addTo(this.map);

    // Add all layer groups to the map
    this.routeLayer.addTo(this.map);
    this.stationLayer.addTo(this.map);
    this.trailLayer.addTo(this.map);
    this.markerLayer.addTo(this.map);

    // Load initial train positions
    this.trainPositionService
      .getTrainPositions()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (positions: IPosition[]) => {
          console.log('Received positions:', positions);
          this.positions = positions;
          this.isLoading = false;
          this.loadError = null;
          this.initializeRoutes();
          this.updateMap();
          this.fitMapBoundsToData(); // Fit bounds only on initial load
        },
        error: (error) => {
          console.error('Failed to load train positions:', error);
          this.isLoading = false;
          this.loadError = `Failed to load train positions: ${error.message || error.statusText || 'Unknown error'}`;
        }
      });

    // Load stations from backend for dropdown
    this.trainPositionService
      .getStations()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stations: IStation[]) => {
          console.log('Loaded stations:', stations);
          this.stations = stations;
        },
        error: (error) => {
          console.error('Failed to load stations:', error);
        }
      });

    // Connect to SignalR hub and subscribe to real-time updates
    this.signalRService
      .start()
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

  private initializeRoutes(): void {
    // Fetch routes from backend instead of hardcoding them
    this.trainPositionService
      .getTrainRoutes()
      .subscribe((backendRoutes: any[]) => {
        console.log('Received routes from backend:', backendRoutes);

        this.positions.forEach((position) => {
          if (!this.routes.has(position.trainId)) {
            // Find the matching route from backend
            // Position trainIds have format "RouteName_TXX", route trainIds are just "RouteName"
            const routeId = position.trainId.includes('_')
              ? position.trainId.substring(0, position.trainId.lastIndexOf('_'))
              : position.trainId;
            const backendRoute = backendRoutes.find(
              (r) => r.trainId === routeId,
            );

            if (backendRoute) {
              this.routes.set(position.trainId, {
                trainId: position.trainId,
                stops: backendRoute.stops,
                currentStopIndex: 0,
              });
            }
          }
        });
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.signalRService
      .stop()
      .catch((error) => console.error('Error stopping SignalR:', error));
  }

  private updateTrainPosition(update: IPosition): void {
    const index = this.positions.findIndex((p) => p.trainId === update.trainId);
    if (index > -1) {
      const existingPosition = this.positions[index];
      const trail = existingPosition.trail || [];
      trail.push([update.latitude, update.longitude]);

      // Keep trail to last 100 points
      if (trail.length > 100) {
        trail.shift();
      }

      this.positions[index] = {
        ...update,
        trail: trail,
      };
    } else {
      this.positions.push({
        ...update,
        trail: [[update.latitude, update.longitude]],
      });
    }
  }

  updateMap() {
    // Clear all layers
    this.markerLayer.clearLayers();
    this.trailLayer.clearLayers();
    this.routeLayer.clearLayers();
    this.stationLayer.clearLayers();

    this.positions.forEach((position) => {
      if (position.latitude && position.longitude) {
        // Draw train trail
        if (position.trail && position.trail.length > 1) {
          const polyline = L.polyline(position.trail, {
            color: '#3498db',
            weight: 2,
            opacity: 0.6,
            dashArray: '5, 5',
          });
          polyline.addTo(this.trailLayer);
        }

        // Draw route and stations
        const route = this.routes.get(position.trainId);
        if (route) {
          route.stops.forEach((stop) => {
            // Add station markers using fixed coordinates from route
            const stationIcon = L.divIcon({
              html: `<div class="station-icon">🚉</div>`,
              iconSize: [30, 30],
              className: 'station-marker',
            });

            const stationMarker = L.marker([stop.latitude, stop.longitude], {
              icon: stationIcon,
            });
            stationMarker.bindPopup(
              `<strong>${stop.stationName}</strong><br/>Distance: ${stop.distance} km`,
            );
            stationMarker.addTo(this.stationLayer);
          });
        }

        // Draw train marker
        const marker = L.marker([position.latitude, position.longitude], {
          icon: this.createTrainIcon(position.status),
          zIndexOffset: 1000,
        });

        const locationDisplay = this.getLocationDisplay(position);
        const popupContent = `
          <div style="font-family: Arial; width: 220px;">
            <h4 style="margin: 5px 0; color: #2c3e50;">${position.trainId}</h4>
            <hr style="margin: 5px 0; border: none; border-top: 1px solid #ecf0f1;">
            <p style="margin: 3px 0;"><strong>Status:</strong> <span style="color: ${position.status.toLowerCase() === 'delayed' ? '#e74c3c' : '#27ae60'};">${position.status}</span></p>
            <p style="margin: 3px 0;"><strong>Location:</strong><br/>${locationDisplay}</p>
            <p style="margin: 3px 0;"><strong>ETA:</strong> ${position.eta}</p>
          </div>
        `;
        marker.bindPopup(popupContent);
        marker.addTo(this.markerLayer);
      }
    });
  }

  private fitMapBoundsToData(): void {
    // Fit map bounds to show all markers - only called on initial load
    const allLayers = [
      ...Array.from(this.markerLayer.getLayers()),
      ...Array.from(this.stationLayer.getLayers()),
    ];

    if (allLayers.length > 0) {
      const group = new L.FeatureGroup(allLayers);
      try {
        this.map.fitBounds(group.getBounds().pad(0.1));
      } catch (e) {
        console.log('Could not fit bounds:', e);
      }
    }
  }

  getRouteInfo(trainId: string): string {
    const route = this.routes.get(trainId);
    if (!route) return 'No route';
    const stops = route.stops.map((s) => s.stationName).join(' → ');
    return stops;
  }

  getLocationDisplay(position: IPosition): string {
    // If train is at a station, show the station name
    if (position.currentStop) {
      return position.currentStop;
    }
    // If train is in-route to a station, show en-route message
    if (position.nextStop) {
      return `en-route → ${position.nextStop}`;
    }
    // Otherwise show coordinates
    return `${position.latitude.toFixed(4)}, ${position.longitude.toFixed(4)}`;
  }

  selectStation(station: string): void {
    this.selectedStation = station;
    this.latitude = null;
    this.longitude = null;
    this.nextArrivalResult = null;

    const match = this.stations.find(s => s.stationName === station);
    if (match) {
      this.selectedStationId = match.id;
      // Place a pin on the selected station
      this.setUserLocationPin(match.latitude, match.longitude, `📍 ${match.stationName}`);
      // Fetch ETA for this station
      this.fetchNextArrivalEta(match.id);
    }
  }

  enableLocation(): void {
    if (!navigator.geolocation) {
      console.error('Geolocation is not supported by this browser.');
      return;
    }

    this.isLoadingLocations = true;
    navigator.geolocation.getCurrentPosition(
      (position) => {
        this.latitude = position.coords.latitude;
        this.longitude = position.coords.longitude;
        this.hasLocationPermission = true;
        this.selectedStation = '';
        this.selectedStationId = null;
        this.nextArrivalResult = null;

        // Add user location pin on the map
        this.setUserLocationPin(this.latitude, this.longitude, '📍 You are here');
        this.map.setView([this.latitude, this.longitude], 14);

        // Fetch ETA based on user coordinates
        this.fetchNextArrivalEta(undefined, this.latitude, this.longitude);
        this.isLoadingLocations = false;
      },
      (error) => {
        console.error('Error getting location:', error);
        this.hasLocationPermission = false;
        this.isLoadingLocations = false;
      },
    );
  }

  private setUserLocationPin(lat: number, lng: number, label: string): void {
    // Remove existing user marker
    if (this.userLocationMarker) {
      this.userLocationMarker.remove();
    }

    const userIcon = L.divIcon({
      html: `<div class="user-location-icon">📍</div>`,
      iconSize: [30, 30],
      className: 'user-location-marker',
    });

    this.userLocationMarker = L.marker([lat, lng], { icon: userIcon, zIndexOffset: 2000 })
      .bindPopup(`<strong>${label}</strong><br/>Lat: ${lat.toFixed(4)}, Lng: ${lng.toFixed(4)}`)
      .addTo(this.map);
  }

  private fetchNextArrivalEta(stopId?: number, latitude?: number, longitude?: number): void {
    this.trainPositionService
      .getNextArrivalEta(stopId, latitude, longitude)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: INextArrivalResult) => {
          console.log('Next arrival ETA:', result);
          this.nextArrivalResult = result;
        },
        error: (error) => {
          console.error('Error fetching next arrival ETA:', error);
          this.nextArrivalResult = null;
        },
      });
  }

}
