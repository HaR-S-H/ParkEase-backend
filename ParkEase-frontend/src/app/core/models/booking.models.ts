export enum BookingType {
  PRE = 0,
  WALK_IN = 1
}

export enum BookingStatus {
  RESERVED = 0,
  ACTIVE = 1,
  COMPLETED = 2,
  CANCELLED = 3
}

export interface CreateBookingRequest {
  userId: number;
  lotId: number;
  spotId: number;
  vehiclePlate: string;
  vehicleType: string;
  bookingType: BookingType;
  startTime: string; // ISO String
  endTime: string; // ISO String
}

export interface ExtendBookingRequest {
  newEndTime: string; // ISO String
}

export interface BookingResponse {
  bookingId: number;
  userId: number;
  lotId: number;
  spotId: number;
  vehiclePlate: string;
  vehicleType: string;
  bookingType: BookingType;
  startTime: string;
  endTime: string;
  checkInTime?: string;
  status: BookingStatus;
  totalAmount: number;
  createdAt: string;
}

export interface CalculateAmountResponse {
  bookingId: number;
  amount: number;
}
