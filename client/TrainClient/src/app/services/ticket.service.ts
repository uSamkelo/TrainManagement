import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PurchaseTicketRequest, PurchaseResult, TicketDTO, TicketAvailabilityDTO } from '../interfaces/ITicket';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly apiUrl = 'http://localhost:5182/api/tickets';

  constructor(private http: HttpClient) {}

  getAvailability(): Observable<TicketAvailabilityDTO[]> {
    return this.http.get<TicketAvailabilityDTO[]>(`${this.apiUrl}/availability`);
  }

  purchase(request: PurchaseTicketRequest): Observable<PurchaseResult> {
    return this.http.post<PurchaseResult>(`${this.apiUrl}/purchase`, request);
  }

  getMyTickets(): Observable<TicketDTO[]> {
    return this.http.get<TicketDTO[]>(this.apiUrl);
  }

  getActiveTickets(): Observable<TicketDTO[]> {
    return this.http.get<TicketDTO[]>(`${this.apiUrl}/active`);
  }

  getTicket(id: string): Observable<TicketDTO> {
    return this.http.get<TicketDTO>(`${this.apiUrl}/${id}`);
  }

  validateTicket(id: string): Observable<TicketDTO> {
    return this.http.post<TicketDTO>(`${this.apiUrl}/${id}/validate`, {});
  }

  cancelTicket(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/cancel`, {});
  }
}
