import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ParkingLotService } from '@/app/core/services/parking-lot.service';
import { AuthSessionService } from '@/app/core/services/auth-session.service';
import { ParkingLotResponse } from '@/app/core/models/parking-lot.models';

@Component({
  selector: 'app-driver-dashboard-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './driver-dashboard-page.component.html',
  styleUrl: './driver-dashboard-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DriverDashboardPageComponent implements OnInit {
  private parkingLotService = inject(ParkingLotService);
  private session = inject(AuthSessionService);
  private router = inject(Router);

  user = this.session.user;
  lots = signal<ParkingLotResponse[]>([]);
  loading = signal(true);
  searchQuery = signal('');

  ngOnInit() {
    this.loadAllLots();
  }

  loadAllLots() {
    this.loading.set(true);
    this.parkingLotService.getLots().subscribe({
      next: (res) => {
        // Only show approved and open lots to drivers
        this.lots.set(res.filter(l => l.isApproved && l.isOpen));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onSearch() {
    if (!this.searchQuery().trim()) {
      this.loadAllLots();
      return;
    }
    
    this.loading.set(true);
    this.parkingLotService.searchLots(this.searchQuery()).subscribe({
      next: (res) => {
        this.lots.set(res.filter(l => l.isApproved && l.isOpen));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  logout() {
    this.session.clearSession();
    this.router.navigateByUrl('/auth/login');
  }

  bookSpot(lotId: number) {
    this.router.navigate(['/dashboard/driver/book', lotId]);
  }
}
