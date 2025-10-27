import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { CommentDto } from './comment.service';

@Injectable({ providedIn: 'root' })
export class CommentSignalRService {
  private hubConnection!: signalR.HubConnection;
  private _newComment$ = new BehaviorSubject<CommentDto | null>(null);

  newComment$ = this._newComment$.asObservable();

  startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:8080/hubs/comments')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('SignalR Connected'))
      .catch(err => console.error('SignalR Connection Error:', err));

    // Подписка на событие от сервера
    this.hubConnection.on('ReceiveComment', (comment: CommentDto) => {
      console.log('New comment received:', comment);
      this._newComment$.next(comment);
    });
  }
}
