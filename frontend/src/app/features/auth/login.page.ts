import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule],
  template: `
    <div class="card" style="max-width: 420px; margin: 40px auto;">
      <h2>Sign in</h2>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <div class="field">
          <label for="email">Email</label>
          <input id="email" type="email" formControlName="email" autocomplete="username" />
        </div>
        <div class="field">
          <label for="password">Password</label>
          <input id="password" type="password" formControlName="password" autocomplete="current-password" />
        </div>
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
        <button class="primary" type="submit" [disabled]="form.invalid || busy()">Sign in</button>
      </form>
      <p style="color: #6b7280; font-size: 13px; margin-top: 12px;">Development: admin&#64;developerstore.local / Admin&#64;123</p>
    </div>
  `,
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    email: ['admin@developerstore.local', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });
  readonly error = signal<string | null>(null);
  readonly busy = signal(false);

  async submit(): Promise<void> {
    this.busy.set(true);
    this.error.set(null);
    try {
      const { email, password } = this.form.getRawValue();
      await this.auth.login(email, password);
      await this.router.navigate(['/sales']);
    } catch {
      this.error.set('Invalid credentials');
    } finally {
      this.busy.set(false);
    }
  }
}
