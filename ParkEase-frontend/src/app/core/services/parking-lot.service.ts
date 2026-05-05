import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '@/environments/environment';
import {
  CreateParkingLotRequest,
  ParkingLotResponse,
  UpdateParkingLotRequest,
} from '@/app/core/models/parking-lot.models';

@Injectable({ providedIn: 'root' })
export class ParkingLotService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiGatewayBaseUrl}/lots`;

  createLot(payload: CreateParkingLotRequest) {
    const formData = new FormData();
    formData.append('name', payload.name);
    formData.append('address', payload.address);
    formData.append('city', payload.city);
    formData.append('latitude', payload.latitude.toString());
    formData.append('longitude', payload.longitude.toString());
    formData.append('totalSpots', payload.totalSpots.toString());
    formData.append('managerId', payload.managerId.toString());
    formData.append('isOpen', payload.isOpen.toString());
    formData.append('isApproved', payload.isApproved.toString());
    formData.append('openTime', payload.openTime);
    formData.append('closeTime', payload.closeTime);

    if (payload.availableSpots !== undefined) {
      formData.append('availableSpots', payload.availableSpots.toString());
    }

    if (payload.imageFile) {
      formData.append('imageFile', payload.imageFile);
    }

    return this.http.post<ParkingLotResponse>(this.baseUrl, formData);
  }

  getLotById(lotId: number) {
    return this.http.get<ParkingLotResponse>(`${this.baseUrl}/${lotId}`);
  }

  getLots() {
    return this.http.get<ParkingLotResponse[]>(this.baseUrl);
  }

  getLotsByCity(city: string) {
    return this.http.get<ParkingLotResponse[]>(`${this.baseUrl}/city/${city}`);
  }

  getLotsByManager(managerId: number) {
    return this.http.get<ParkingLotResponse[]>(`${this.baseUrl}/manager/${managerId}`);
  }

  getNearbyLots(latitude: number, longitude: number, radiusKm: number = 5) {
    let params = new HttpParams()
      .set('latitude', latitude.toString())
      .set('longitude', longitude.toString())
      .set('radiusKm', radiusKm.toString());
    return this.http.get<ParkingLotResponse[]>(`${this.baseUrl}/nearby`, { params });
  }

  searchLots(query: string) {
    let params = new HttpParams().set('query', query);
    return this.http.get<ParkingLotResponse[]>(`${this.baseUrl}/search`, { params });
  }

  updateLot(lotId: number, payload: UpdateParkingLotRequest) {
    const formData = new FormData();
    if (payload.name) formData.append('name', payload.name);
    if (payload.address) formData.append('address', payload.address);
    if (payload.city) formData.append('city', payload.city);
    if (payload.latitude !== undefined) formData.append('latitude', payload.latitude.toString());
    if (payload.longitude !== undefined) formData.append('longitude', payload.longitude.toString());
    if (payload.totalSpots !== undefined) formData.append('totalSpots', payload.totalSpots.toString());
    if (payload.availableSpots !== undefined) formData.append('availableSpots', payload.availableSpots.toString());
    if (payload.managerId !== undefined) formData.append('managerId', payload.managerId.toString());
    if (payload.isOpen !== undefined) formData.append('isOpen', payload.isOpen.toString());
    if (payload.isApproved !== undefined) formData.append('isApproved', payload.isApproved.toString());
    if (payload.openTime) formData.append('openTime', payload.openTime);
    if (payload.closeTime) formData.append('closeTime', payload.closeTime);

    if (payload.imageFile) {
      formData.append('imageFile', payload.imageFile);
    }

    return this.http.put<ParkingLotResponse>(`${this.baseUrl}/${lotId}`, formData);
  }

  toggleOpen(lotId: number) {
    return this.http.put<ParkingLotResponse>(`${this.baseUrl}/${lotId}/toggle-open`, {});
  }

  deleteLot(lotId: number) {
    return this.http.delete<void>(`${this.baseUrl}/${lotId}`);
  }
}
