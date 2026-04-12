import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { TrainUpdate } from '../interfaces/IPositions';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection;
  private trainUpdateSubject = new Subject<TrainUpdate>();
  public trainUpdate$ = this.trainUpdateSubject.asObservable();

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5176/train-update')
      .withAutomaticReconnect([1000, 3000, 5000, 10000])
      .build();

    this.setupListeners();
  }

  private setupListeners() {
    this.hubConnection.on('ReceiveTrainUpdate', (update: TrainUpdate) => {
      console.log('Received train update via SignalR:', update);
      this.trainUpdateSubject.next(update);
    });

    this.hubConnection.on('Connected', (message: string) => {
      console.log(message);
    });

    this.hubConnection.onreconnecting((error: Error | undefined) => {
      console.log('Attempting to reconnect:', error);
    });

    this.hubConnection.onreconnected((connectionId: string | undefined) => {
      console.log('Reconnected with ID:', connectionId);
    });
  }

  public start(): Promise<void> {
    return this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connection established');
      })
      .catch((error: any) => {
        console.error('Error starting SignalR connection:', error);
        return Promise.reject(error);
      });
  }

  public stop(): Promise<void> {
    return this.hubConnection.stop();
  }

  public async sendTrainUpdate(update: TrainUpdate): Promise<void> {
    return this.hubConnection.invoke('SendTrainUpdate', update);
  }

  public isConnected(): boolean {
    return this.hubConnection.state === signalR.HubConnectionState.Connected;
  }
}
