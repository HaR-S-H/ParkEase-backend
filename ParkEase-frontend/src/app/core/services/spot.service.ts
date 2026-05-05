import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '@/environments/environment';
import { ParkingSpotResponse } from '@/app/core/models/spot.models';

@Injectable({ providedIn: 'root' })
export class SpotService {
  private readonly http = inject(HttpClient);
  // Note: Assuming spots are routed via /api/v1/spots but usually they might be /api/v1/spots/
  private readonly baseUrl = `${environment.apiGatewayBaseUrl}/spots`;

  getAvailableSpots(lotId: number) {
    return this.http.get<ParkingSpotResponse[]>(`${this.baseUrl}/lot/${lotId}/available`);
  }

  getSpotsByLot(lotId: number) {
    return this.http.get<ParkingSpotResponse[]>(`${this.baseUrl}/lot/${lotId}`);
  }
}
