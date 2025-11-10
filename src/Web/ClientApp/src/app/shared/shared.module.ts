import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// Components
// import { LoadingSpinnerComponent } from './components/loading-spinner/loading-spinner.component';
// import { PageHeaderComponent } from './components/page-header/page-header.component';

// Directives
// import { HasRoleDirective } from './directives/has-role.directive';

// Pipes
// import { FileSizePipe } from './pipes/file-size.pipe';

/**
 * Shared Module
 *
 * Contains reusable components, directives, and pipes.
 * Import this module in feature modules that need shared functionality.
 */
@NgModule({
  declarations: [
    // Components
    // LoadingSpinnerComponent,
    // PageHeaderComponent,

    // Directives
    // HasRoleDirective,

    // Pipes
    // FileSizePipe
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule
  ],
  exports: [
    // Angular modules
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,

    // Shared components
    // LoadingSpinnerComponent,
    // PageHeaderComponent,

    // Directives
    // HasRoleDirective,

    // Pipes
    // FileSizePipe
  ]
})
export class SharedModule {}
