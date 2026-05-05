import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '@/environments/environment';
import { RegisterVehicleRequest, UpdateVehicleRequest, VehicleResponse } from '@/app/core/models/vehicle.models';

@Injectable({ providedIn: 'root' })
export class VehicleService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiGatewayBaseUrl}/vehicles`;

  registerVehicle(payload: RegisterVehicleRequest) {
    return this.http.post<VehicleResponse>(`${this.baseUrl}/register`, payload);
  }

  getVehicleById(vehicleId: number) {
    return this.http.get<VehicleResponse>(`${this.baseUrl}/${vehicleId}`);
  }

  getVehiclesByOwner(ownerId: number) {
    return this.http.get<VehicleResponse[]>(`${this.baseUrl}/owner/${ownerId}`);
  }

  getByLicensePlate(licensePlate: string) {
    return this.http.get<VehicleResponse>(`${this.baseUrl}/plate/${licensePlate}`);
  }

  updateVehicle(vehicleId: number, payload: UpdateVehicleRequest) {
    return this.http.put<VehicleResponse>(`${this.baseUrl}/${vehicleId}`, payload);
  }

  deleteVehicle(vehicleId: number) {
    return this.http.delete<void>(`${this.baseUrl}/${vehicleId}`);
  }
}
