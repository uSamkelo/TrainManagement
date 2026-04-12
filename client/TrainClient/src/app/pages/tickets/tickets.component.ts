import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TicketService } from '../../services/ticket.service';
import { TicketAvailabilityDTO, PurchaseTicketRequest } from '../../interfaces/ITicket';

@Component({
  selector: 'app-tickets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tickets.component.html',
  styleUrls: ['./tickets.component.css']
})
export class TicketsComponent implements OnInit {
  availability: TicketAvailabilityDTO[] = [];
  loading = true;
  purchasing = false;
  error = '';
  success = '';

  // Form
  selectedRoute: TicketAvailabilityDTO | null = null;
  fromStation = '';
  toStation = '';
  ticketType = 'OnceOff';
  paymentMethod = 'card';

  ticketTypes = [
    { value: 'OnceOff', label: 'Once-Off', icon: '🎟️', desc: 'Single trip valid for 1 day' },
    { value: 'Weekly', label: 'Weekly Pass', icon: '📅', desc: 'Unlimited trips for 7 days' },
    { value: 'Monthly', label: 'Monthly Pass', icon: '🗓️', desc: 'Unlimited trips for 30 days' }
  ];

  paymentMethods = [
    { value: 'card', label: 'Card', icon: '💳' },
    { value: 'eft', label: 'EFT', icon: '🏦' },
    { value: 'cash', label: 'Cash', icon: '💵' }
  ];

  constructor(private ticketService: TicketService, private router: Router) {}

  ngOnInit(): void {
    this.ticketService.getAvailability().subscribe({
      next: (data) => { this.availability = data; this.loading = false; },
      error: () => { this.loading = false; this.error = 'Failed to load route availability.'; }
    });
  }

  selectRoute(route: TicketAvailabilityDTO): void {
    this.selectedRoute = route;
    this.fromStation = '';
    this.toStation = '';
    this.success = '';
    this.error = '';
  }

  get selectedPrice(): number {
    if (!this.selectedRoute) return 0;
    return this.selectedRoute.pricing[this.ticketType] ?? 0;
  }

  get canPurchase(): boolean {
    return !!(this.selectedRoute && this.fromStation && this.toStation &&
      this.fromStation !== this.toStation && this.ticketType && this.paymentMethod);
  }

  purchase(): void {
    if (!this.canPurchase || !this.selectedRoute) return;

    this.purchasing = true;
    this.error = '';
    this.success = '';

    const request: PurchaseTicketRequest = {
      ticketType: this.ticketType,
      routeId: this.selectedRoute.routeId,
      fromStation: this.fromStation,
      toStation: this.toStation,
      paymentMethod: this.paymentMethod
    };

    this.ticketService.purchase(request).subscribe({
      next: (result) => {
        this.purchasing = false;
        this.success = `Ticket purchased! Ref: ${result.payment.transactionRef}`;
        setTimeout(() => this.router.navigate(['/my-tickets']), 2000);
      },
      error: (err) => {
        this.purchasing = false;
        this.error = err.error?.error || 'Purchase failed. Please try again.';
      }
    });
  }
}
