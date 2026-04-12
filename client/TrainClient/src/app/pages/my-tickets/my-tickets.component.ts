import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TicketService } from '../../services/ticket.service';
import { TicketDTO } from '../../interfaces/ITicket';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './my-tickets.component.html',
  styleUrls: ['./my-tickets.component.css']
})
export class MyTicketsComponent implements OnInit {
  tickets: TicketDTO[] = [];
  loading = true;
  error = '';
  filter: 'all' | 'active' | 'expired' = 'all';
  selectedTicket: TicketDTO | null = null;

  constructor(private ticketService: TicketService, public auth: AuthService) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.loading = true;
    this.ticketService.getMyTickets().subscribe({
      next: (tickets) => { this.tickets = tickets; this.loading = false; },
      error: () => { this.loading = false; this.error = 'Failed to load tickets.'; }
    });
  }

  get filteredTickets(): TicketDTO[] {
    if (this.filter === 'active') return this.tickets.filter(t => t.status === 'Active');
    if (this.filter === 'expired') return this.tickets.filter(t => t.status !== 'Active');
    return this.tickets;
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Active': return '#27ae60';
      case 'Used': return '#7f8c8d';
      case 'Expired': return '#e74c3c';
      case 'Cancelled': return '#95a5a6';
      case 'Refunded': return '#f39c12';
      default: return '#7f8c8d';
    }
  }

  getTypeIcon(type: string): string {
    switch (type) {
      case 'OnceOff': return '🎟️';
      case 'Weekly': return '📅';
      case 'Monthly': return '🗓️';
      default: return '🎫';
    }
  }

  viewTicket(ticket: TicketDTO): void {
    this.selectedTicket = this.selectedTicket?.id === ticket.id ? null : ticket;
  }

  cancelTicket(id: string): void {
    if (!confirm('Are you sure you want to cancel this ticket?')) return;
    this.ticketService.cancelTicket(id).subscribe({
      next: () => this.loadTickets(),
      error: () => this.error = 'Failed to cancel ticket.'
    });
  }

  isValid(ticket: TicketDTO): boolean {
    return new Date(ticket.validUntil) > new Date() && ticket.status === 'Active';
  }

  getStationCode(stationName: string): string {
    if (!stationName) return '---';
    // Generate a 3-letter code from the station name
    const words = stationName.replace(/['']/g, '').split(/[\s-]+/);
    if (words.length === 1) {
      return words[0].substring(0, 3).toUpperCase();
    }
    // Take first letter of each word, pad to 3
    return words.map(w => w[0]).join('').substring(0, 3).toUpperCase();
  }
}
