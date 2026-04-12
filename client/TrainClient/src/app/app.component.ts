import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TrainPositionComponent } from "./train-position/train-position.component";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, TrainPositionComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'TrainClient';
}
