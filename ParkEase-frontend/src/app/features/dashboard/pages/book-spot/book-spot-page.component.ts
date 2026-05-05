import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BookingService } from '@/app/core/services/booking.service';
import { AuthSessionService } from '@/app/core/services/auth-session.service';
import { CreateBookingRequest, BookingType } from '@/app/core/models/booking.models';
import { SpotService } from '@/app/core/services/spot.service';
import { ParkingSpotResponse, SpotStatus } from '@/app/core/models/spot.models';

@Component({
  selector: 'app-book-spot-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './book-spot-page.component.html',
  styleUrl: './book-spot-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookSpotPageComponent implements OnInit {
  private bookingService = inject(BookingService);
  private spotService = inject(SpotService);
  private session = inject(AuthSessionService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  lotId = Number(this.route.snapshot.paramMap.get('lotId'));

  spots = signal<ParkingSpotResponse[]>([]);
  spotsLoading = signal(true);
  
  SpotStatus = SpotStatus;

  formData = {
    spotId: null as number | null,
    vehiclePlate: '',
    vehicleType: 'FOURW',
    startTime: '',
    endTime: ''
  };

  loading = signal(false);
  error = signal('');

  ngOnInit() {
    this.loadSpots();
  }

  loadSpots() {
    this.spotService.getSpotsByLot(this.lotId).subscribe({
      next: (res) => {
        // Sort spots so they appear in a predictable order
        res.sort((a, b) => a.spotNumber.localeCompare(b.spotNumber, undefined, { numeric: true }));
        this.spots.set(res);
        this.spotsLoading.set(false);
      },
      error: () => {
        this.error.set('Failed to load parking spots. Please try again.');
        this.spotsLoading.set(false);
      }
    });
  }

  selectSpot(spot: ParkingSpotResponse) {
    if (spot.status !== SpotStatus.AVAILABLE) return;
    this.formData.spotId = spot.spotId;
  }

  onSubmit() {
    const data = this.formData;
    if (!data.spotId || !data.vehiclePlate || !data.startTime || !data.endTime) {
      this.error.set('Please select a spot and fill all required fields.');
      return;
    }

    const payload: CreateBookingRequest = {
      userId: Number(this.session.user()?.userId),
      lotId: this.lotId,
      spotId: data.spotId,
      vehiclePlate: data.vehiclePlate,
      vehicleType: data.vehicleType,
      bookingType: BookingType.PRE,
      startTime: new Date(data.startTime).toISOString(),
      endTime: new Date(data.endTime).toISOString()
    };

    this.loading.set(true);
    this.error.set('');

    this.bookingService.createBooking(payload).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/dashboard/driver/bookings']);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to create booking.');
        this.loading.set(false);
      }
    });
  }

  cancel() {
    this.router.navigate(['/dashboard/driver']);
  }
}
