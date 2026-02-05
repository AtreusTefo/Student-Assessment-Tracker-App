import { Component, inject } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  title = 'Student Assessment Tracker';
  private router = inject(Router);

  navigateToList() {
    this.router.navigate(['/']);
  }

  navigateToCreate() {
    this.router.navigate(['/create']);
  }
}
