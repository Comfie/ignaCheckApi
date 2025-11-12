import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faEnvelope,
  faSpinner,
  faArrowLeft,
  faCheckCircle,
  faEnvelopeOpenText,
  faShieldAlt
} from '@fortawesome/free-solid-svg-icons';
import { NotificationService } from '../../../core/services/notification.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    FontAwesomeModule
  ],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent implements OnInit {
  // FontAwesome Icons
  faEnvelope = faEnvelope;
  faSpinner = faSpinner;
  faArrowLeft = faArrowLeft;
  faCheckCircle = faCheckCircle;
  faEnvelopeOpenText = faEnvelopeOpenText;
  faShieldAlt = faShieldAlt;

  forgotPasswordForm!: FormGroup;
  isLoading = false;
  emailSent = false;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  get email() {
    return this.forgotPasswordForm.get('email');
  }

  onSubmit(): void {
    if (this.forgotPasswordForm.invalid) {
      this.forgotPasswordForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const { email } = this.forgotPasswordForm.value;

    // Call forgot password API
    this.http.post(`${environment.apiUrl}/authentication/forgot-password`, { email }).subscribe({
      next: () => {
        this.emailSent = true;
        this.isLoading = false;
        this.notificationService.success('Password reset instructions have been sent to your email.');
      },
      error: (error) => {
        this.isLoading = false;
        this.notificationService.error(error.message || 'Failed to send reset email. Please try again.');
      }
    });
  }

  onBackToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  onResendEmail(): void {
    this.emailSent = false;
    this.onSubmit();
  }
}
