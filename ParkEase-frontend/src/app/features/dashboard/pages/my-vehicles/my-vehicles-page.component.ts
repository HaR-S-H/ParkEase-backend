import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { VehicleService } from '@/app/core/services/vehicle.service';
import { AuthSessionService } from '@/app/core/services/auth-session.service';
import { VehicleResponse, RegisterVehicleRequest } from '@/app/core/models/vehicle.models';

@Component({
  selector: 'app-my-vehicles-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './my-vehicles-page.component.html',
  styleUrl: './my-vehicles-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyVehiclesPageComponent implements OnInit {
  private vehicleService = inject(VehicleService);
  private session = inject(AuthSessionService);

  vehicles = signal<VehicleResponse[]>([]);
  loading = signal(true);
  error = signal('');
  
  showAddForm = signal(false);
  adding = signal(false);

  newVehicle = signal({
    licensePlate: '',
    make: '',
    model: '',
    color: '',
    vehicleType: 'FOURW',
    isEV: false
  });

  ngOnInit() {
    this.loadVehicles();
  }

  loadVehicles() {
    const ownerId = Number(this.session.user()?.userId);
    if (!ownerId) return;

    this.loading.set(true);
    this.vehicleService.getVehiclesByOwner(ownerId).subscribe({
      next: (res) => {
        this.vehicles.set(res.filter(v => v.isActive));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load vehicles.');
        this.loading.set(false);
      }
    });
  }

  deleteVehicle(vehicleId: number) {
    if (!confirm('Are you sure you want to remove this vehicle?')) return;
    this.vehicleService.deleteVehicle(vehicleId).subscribe({
      next: () => this.loadVehicles(),
      error: () => alert('Failed to delete vehicle.')
    });
  }

  registerVehicle() {
    const ownerId = Number(this.session.user()?.userId);
    const data = this.newVehicle();
    
    if (!data.licensePlate || !data.make || !data.model) {
      alert('Please fill the required fields.');
      return;
    }

    const payload: RegisterVehicleRequest = {
      ownerId,
      ...data
    };

    this.adding.set(true);
    this.vehicleService.registerVehicle(payload).subscribe({
      next: () => {
        this.adding.set(false);
        this.showAddForm.set(false);
        this.newVehicle.set({ licensePlate: '', make: '', model: '', color: '', vehicleType: 'FOURW', isEV: false });
        this.loadVehicles();
      },
      error: (err) => {
        alert(err.error?.message || 'Failed to register vehicle.');
        this.adding.set(false);
      }
    });
  }
}
