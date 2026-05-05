import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BookingService } from '@/app/core/services/booking.service';
import { AuthSessionService } from '@/app/core/services/auth-session.service';
import { BookingResponse, BookingStatus } from '@/app/core/models/booking.models';

@Component({
  selector: 'app-my-bookings-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './my-bookings-page.component.html',
  styleUrl: './my-bookings-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyBookingsPageComponent implements OnInit {
  private bookingService = inject(BookingService);
  private session = inject(AuthSessionService);

  bookings = signal<BookingResponse[]>([]);
  loading = signal(true);
  error = signal('');
  
  // Expose enum to template
  BookingStatus = BookingStatus;

  ngOnInit() {
    this.loadBookings();
  }

  loadBookings() {
    const userId = Number(this.session.user()?.userId);
    if (!userId) return;

    this.loading.set(true);
    this.bookingService.getBookingsByUser(userId).subscribe({
      next: (res) => {
        // Sort newest first
        res.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        this.bookings.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load bookings.');
        this.loading.set(false);
      }
    });
  }

  cancelBooking(bookingId: number) {
    if (!confirm('Are you sure you want to cancel this booking?')) return;
    this.bookingService.cancelBooking(bookingId).subscribe({
      next: () => this.loadBookings(),
      error: () => alert('Failed to cancel booking.')
    });
  }

  checkIn(bookingId: number) {
    this.bookingService.checkIn(bookingId).subscribe({
      next: () => this.loadBookings(),
      error: (err) => alert(err.error?.message || 'Check-in failed.')
    });
  }

  checkOut(bookingId: number) {
    this.bookingService.checkOut(bookingId).subscribe({
      next: () => this.loadBookings(),
      error: (err) => alert(err.error?.message || 'Check-out failed.')
    });
  }
}
