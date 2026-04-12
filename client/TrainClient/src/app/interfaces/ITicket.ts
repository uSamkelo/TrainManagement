export interface PurchaseTicketRequest {
  ticketType: string;
  routeId: string;
  fromStation: string;
  toStation: string;
  paymentMethod: string;
  travelDate?: string;
}

export interface TicketDTO {
  id: string;
  type: string;
  status: string;
  routeId: string;
  routeName: string;
  fromStation: string;
  toStation: string;
  validFrom: string;
  validUntil: string;
  price: number;
  qrCode?: string;
  createdAt: string;
}

export interface PaymentDTO {
  id: string;
  ticketId: string;
  amount: number;
  currency: string;
  status: string;
  paymentMethod: string;
  transactionRef?: string;
  createdAt: string;
  completedAt?: string;
}

export interface PurchaseResult {
  ticket: TicketDTO;
  payment: PaymentDTO;
}

export interface TicketAvailabilityDTO {
  routeId: string;
  routeName: string;
  stations: string[];
  pricing: { [key: string]: number };
  available: boolean;
}
