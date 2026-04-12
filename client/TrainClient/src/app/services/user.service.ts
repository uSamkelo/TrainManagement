import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserDTO, UpdateUserRequest } from '../interfaces/IAuth';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly apiUrl = 'http://localhost:5180/api/users';

  constructor(private http: HttpClient) {}

  getAll(): Observable<UserDTO[]> {
    return this.http.get<UserDTO[]>(this.apiUrl);
  }

  getById(id: string): Observable<UserDTO> {
    return this.http.get<UserDTO>(`${this.apiUrl}/${id}`);
  }

  getByRole(role: string): Observable<UserDTO[]> {
    return this.http.get<UserDTO[]>(`${this.apiUrl}/role/${role}`);
  }

  getDrivers(): Observable<UserDTO[]> {
    return this.http.get<UserDTO[]>(`${this.apiUrl}/drivers`);
  }

  getStationStaff(): Observable<UserDTO[]> {
    return this.http.get<UserDTO[]>(`${this.apiUrl}/station-staff`);
  }

  update(id: string, request: UpdateUserRequest): Observable<UserDTO> {
    return this.http.put<UserDTO>(`${this.apiUrl}/${id}`, request);
  }

  deactivate(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
