import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IPosition, IRoute, IStation, INextArrivalResult } from '../interfaces/IPositions';

@Injectable({
  providedIn: 'root',
})
export class TrainPositionService {
  private apiUrl = 'http://localhost:5176/api/trainposition';
  private routeUrl = 'http://localhost:5176/api/trainroute';
  private scheduleUrl = 'http://localhost:5176/api/trainschedule';

  constructor(private http: HttpClient) {}

  getTrainPositions(): Observable<IPosition[]> {
    return this.http.get<IPosition[]>(this.apiUrl);
  }

  getTrainRoutes(): Observable<any[]> {
    return this.http.get<any[]>(this.routeUrl);
  }

  getStations(): Observable<IStation[]> {
    return this.http.get<IStation[]>(`${this.routeUrl}/stations`);
  }

  getNextArrivalEta(stopId?: number, latitude?: number, longitude?: number): Observable<INextArrivalResult> {
    const params: string[] = [];
    if (stopId != null) params.push(`stopId=${stopId}`);
    if (latitude != null) params.push(`latitude=${latitude}`);
    if (longitude != null) params.push(`longitude=${longitude}`);
    const query = params.length ? `?${params.join('&')}` : '';
    return this.http.get<INextArrivalResult>(`${this.scheduleUrl}/next-arrival-eta${query}`);
  }
}
