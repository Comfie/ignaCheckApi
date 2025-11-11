import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * Root Application Component
 * Standalone component that serves as the entry point for the application
 */
@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.css',
    standalone: true,
    imports: [RouterOutlet]
})
export class AppComponent {
  title = 'IgnaCheck - AI-Powered Compliance Audit Platform';
}
