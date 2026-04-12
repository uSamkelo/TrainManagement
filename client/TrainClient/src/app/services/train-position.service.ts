import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IPosition } from '../interfaces/IPositions';

@Injectable({
  providedIn: 'root',
})
export class TrainPositionService {
  private apiUrl = 'http://localhost:5176/api/trainposition';

  constructor(private http: HttpClient) {}

  getTrainPositions(): Observable<IPosition[]> {
    return this.http.get<IPosition[]>(this.apiUrl);
  }
}
