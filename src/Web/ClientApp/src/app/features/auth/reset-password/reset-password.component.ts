import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container-fluid p-0">
      <div class="row m-0">
        <div class="col-12 p-0">
          <div class="login-card login-dark">
            <div>
              <div>
                <a class="logo" routerLink="/">
                  <img class="img-fluid for-dark" src="assets/riho/images/logo/logo.png" alt="IgnaCheck Logo">
                </a>
              </div>
              <div class="login-main">
                <h4>Reset Password</h4>
                <p>Coming soon...</p>
                <p class="mt-4 mb-0">
                  Remember your password?
                  <a class="ms-2" routerLink="/auth/login">Sign in</a>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class ResetPasswordComponent {}
