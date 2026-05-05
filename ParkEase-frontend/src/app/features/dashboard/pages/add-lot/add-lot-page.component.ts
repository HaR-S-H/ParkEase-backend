import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ParkingLotService } from '@/app/core/services/parking-lot.service';
import { AuthSessionService } from '@/app/core/services/auth-session.service';
import { CreateParkingLotRequest } from '@/app/core/models/parking-lot.models';

@Component({
  selector: 'app-add-lot-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './add-lot-page.component.html',
  styleUrl: './add-lot-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddLotPageComponent {
  private parkingLotService = inject(ParkingLotService);
  private session = inject(AuthSessionService);
  private router = inject(Router);

  name = signal('');
  address = signal('');
  city = signal('');
  latitude = signal<number | null>(null);
  longitude = signal<number | null>(null);
  totalSpots = signal<number | null>(null);
  openTime = signal('08:00');
  closeTime = signal('22:00');
  isOpen = signal(true);
  selectedImage = signal<File | null>(null);

  loading = signal(false);
  errorMessage = signal('');

  onImageSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      this.selectedImage.set(file);
    }
  }

  onSubmit() {
    this.errorMessage.set('');
    
    if (!this.name() || !this.address() || !this.city() || !this.latitude() || !this.longitude() || !this.totalSpots()) {
      this.errorMessage.set('Please fill out all required fields.');
      return;
    }

    const managerId = Number(this.session.user()?.userId);
    if (!managerId) {
      this.errorMessage.set('Manager ID not found.');
      return;
    }

    this.loading.set(true);

    const payload: CreateParkingLotRequest = {
      name: this.name(),
      address: this.address(),
      city: this.city(),
      latitude: this.latitude()!,
      longitude: this.longitude()!,
      totalSpots: this.totalSpots()!,
      managerId,
      isOpen: this.isOpen(),
      isApproved: false,
      openTime: this.openTime() + ':00', // Ensure HH:mm:ss if backend parses strictly, though HH:mm usually works
      closeTime: this.closeTime() + ':00',
      imageFile: this.selectedImage() || undefined
    };

    this.parkingLotService.createLot(payload).subscribe({
      next: () => {
        this.router.navigateByUrl('/dashboard/manager');
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to create parking lot.');
      }
    });
  }
}
