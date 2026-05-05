import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '@/environments/environment';
import {
  CreateBookingRequest,
  BookingResponse,
  ExtendBookingRequest,
  CalculateAmountResponse
} from '@/app/core/models/booking.models';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiGatewayBaseUrl}/bookings`;

  createBooking(payload: CreateBookingRequest) {
    return this.http.post<BookingResponse>(this.baseUrl, payload);
  }

  getBookingById(bookingId: number) {
    return this.http.get<BookingResponse>(`${this.baseUrl}/${bookingId}`);
  }

  getBookingsByUser(userId: number) {
    return this.http.get<BookingResponse[]>(`${this.baseUrl}/user/${userId}`);
  }

  getBookingsByLot(lotId: number) {
    return this.http.get<BookingResponse[]>(`${this.baseUrl}/lot/${lotId}`);
  }

  getActiveBookings() {
    return this.http.get<BookingResponse[]>(`${this.baseUrl}/active`);
  }

  cancelBooking(bookingId: number) {
    return this.http.put<void>(`${this.baseUrl}/${bookingId}/cancel`, {});
  }

  checkIn(bookingId: number) {
    return this.http.put<BookingResponse>(`${this.baseUrl}/${bookingId}/checkin`, {});
  }

  checkOut(bookingId: number) {
    return this.http.put<BookingResponse>(`${this.baseUrl}/${bookingId}/checkout`, {});
  }

  extendBooking(bookingId: number, payload: ExtendBookingRequest) {
    return this.http.put<BookingResponse>(`${this.baseUrl}/${bookingId}/extend`, payload);
  }

  calculateAmount(bookingId: number) {
    return this.http.get<CalculateAmountResponse>(`${this.baseUrl}/${bookingId}/calculate-amount`);
  }

  getBookingHistory(userId: number) {
    return this.http.get<BookingResponse[]>(`${this.baseUrl}/user/${userId}/history`);
  }
}
