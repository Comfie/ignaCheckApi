import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <footer class="footer">
      <div class="container-fluid">
        <div class="row">
          <div class="col-md-6 p-0 footer-left">
            <p class="mb-0">Copyright {{ currentYear }} © IgnaCheck.ai</p>
          </div>
          <div class="col-md-6 p-0 footer-right">
            <p class="mb-0">
              AI-Powered Compliance Audit Platform
              <i class="fa fa-heart font-danger"></i>
            </p>
          </div>
        </div>
      </div>
    </footer>
  `,
  styles: [`
    .footer {
      padding: 20px 0;
      border-top: 1px solid #e0e0e0;
      margin-top: auto;

      .footer-left p {
        color: #6c757d;
        font-size: 14px;
      }

      .footer-right p {
        color: #6c757d;
        font-size: 14px;
        text-align: right;
      }

      .font-danger {
        color: #dc3545;
      }
    }
  `]
})
export class FooterComponent {
  currentYear = new Date().getFullYear();
}
