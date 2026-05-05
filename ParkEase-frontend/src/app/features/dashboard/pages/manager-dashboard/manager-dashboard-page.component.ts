import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthSessionService } from '@/app/core/services/auth-session.service';
import { ParkingLotService } from '@/app/core/services/parking-lot.service';
import { ParkingLotResponse } from '@/app/core/models/parking-lot.models';

@Component({
  selector: 'app-manager-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './manager-dashboard-page.component.html',
  styleUrl: './manager-dashboard-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ManagerDashboardPageComponent implements OnInit {
  private session = inject(AuthSessionService);
  private router = inject(Router);
  private parkingLotService = inject(ParkingLotService);

  user = this.session.user;
  lots = signal<ParkingLotResponse[]>([]);
  loading = signal(true);
  error = signal('');

  ngOnInit() {
    const managerId = this.user()?.userId;
    if (managerId) {
      this.parkingLotService.getLotsByManager(Number(managerId)).subscribe({
        next: (res) => {
          this.lots.set(res);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set('Failed to load your parking lots.');
          this.loading.set(false);
        }
      });
    } else {
      this.loading.set(false);
    }
  }

  toggleLotStatus(lot: ParkingLotResponse) {
    this.parkingLotService.toggleOpen(lot.lotId).subscribe({
      next: (res) => {
        this.lots.update(lots => lots.map(l => l.lotId === res.lotId ? res : l));
      }
    });
  }

  logout() {
    this.session.clearSession();
    this.router.navigateByUrl('/auth/login');
  }
}
