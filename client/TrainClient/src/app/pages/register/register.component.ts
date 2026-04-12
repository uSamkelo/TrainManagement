import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RegisterRequest } from '../../interfaces/IAuth';
import { IStation } from '../../interfaces/IPositions';
import { TrainPositionService } from '../../services/train-position.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // Form steps
  currentStep = 1;
  totalSteps = 2;

  // Role selection
  selectedRole = 'Passenger';
  roles = [
    { value: 'Passenger', label: 'Passenger', icon: '🧑‍💼', desc: 'View schedules, track trains & buy tickets', color: '#3498db' },
    { value: 'Driver', label: 'Train Driver', icon: '🚂', desc: 'Operate trains & update trip status', color: '#e67e22', requiresAuth: true },
    { value: 'StationStaff', label: 'Station Staff', icon: '🎫', desc: 'Manage tickets, assist passengers & update statuses', color: '#27ae60', requiresAuth: true },
    { value: 'Admin', label: 'Administrator', icon: '🛡️', desc: 'Full system access & user management', color: '#e74c3c', requiresAuth: true }
  ];

  // Form fields
  email = '';
  password = '';
  confirmPassword = '';
  firstName = '';
  lastName = '';
  phoneNumber = '';
  employeeId = '';
  assignedStationId = '';
  assignedTrainId = '';
  adminCode = '';

  // Staff auth codes (in a real app, these would be server-side validated)
  readonly STAFF_AUTH_CODES: { [key: string]: string } = {
    Driver: 'DRIVER2026',
    StationStaff: 'STAFF2026',
    Admin: 'ADMIN2026'
  };

  // Station data from API
  stations: IStation[] = [];
  trainIds = ['Train-Southern-1', 'Train-Southern-2', 'Train-Northern-1', 'Train-Northern-2', 'Train-CapeFlats-1', 'Train-CapeFlats-2'];

  // UI state
  loading = false;
  error = '';
  passwordStrength = 0;
  showPassword = false;
  showAdminCode = false;

  constructor(
    private authService: AuthService,
    private trainService: TrainPositionService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (this.authService.isLoggedIn) {
      this.router.navigate(['/dashboard']);
      return;
    }
    this.trainService.getStations().subscribe({
      next: (stations) => this.stations = stations,
      error: () => {}
    });
  }

  get selectedRoleInfo() {
    return this.roles.find(r => r.value === this.selectedRole)!;
  }

  get isStaffRole(): boolean {
    return this.selectedRole !== 'Passenger';
  }

  get needsStation(): boolean {
    return this.selectedRole === 'StationStaff';
  }

  get needsTrain(): boolean {
    return this.selectedRole === 'Driver';
  }

  get passwordsMatch(): boolean {
    return this.password === this.confirmPassword;
  }

  get step1Valid(): boolean {
    if (!this.selectedRole) return false;
    if (this.isStaffRole && !this.adminCode) return false;
    if (this.isStaffRole && this.adminCode !== this.STAFF_AUTH_CODES[this.selectedRole]) return false;
    return true;
  }

  get step2Valid(): boolean {
    return !!(this.email && this.password && this.password.length >= 8 &&
      this.passwordsMatch && this.firstName && this.lastName);
  }

  selectRole(role: string): void {
    this.selectedRole = role;
    this.adminCode = '';
    this.employeeId = '';
    this.assignedStationId = '';
    this.assignedTrainId = '';
    this.error = '';
  }

  checkPasswordStrength(): void {
    let score = 0;
    if (this.password.length >= 8) score++;
    if (this.password.length >= 12) score++;
    if (/[A-Z]/.test(this.password)) score++;
    if (/[0-9]/.test(this.password)) score++;
    if (/[^A-Za-z0-9]/.test(this.password)) score++;
    this.passwordStrength = score;
  }

  get passwordStrengthLabel(): string {
    if (this.passwordStrength <= 1) return 'Weak';
    if (this.passwordStrength <= 2) return 'Fair';
    if (this.passwordStrength <= 3) return 'Good';
    return 'Strong';
  }

  get passwordStrengthColor(): string {
    if (this.passwordStrength <= 1) return '#e74c3c';
    if (this.passwordStrength <= 2) return '#f39c12';
    if (this.passwordStrength <= 3) return '#3498db';
    return '#27ae60';
  }

  nextStep(): void {
    if (this.currentStep < this.totalSteps) this.currentStep++;
  }

  prevStep(): void {
    if (this.currentStep > 1) this.currentStep--;
  }

  register(): void {
    if (!this.step2Valid) return;

    this.loading = true;
    this.error = '';

    const request: RegisterRequest = {
      email: this.email,
      password: this.password,
      firstName: this.firstName,
      lastName: this.lastName,
      role: this.selectedRole,
      phoneNumber: this.phoneNumber || undefined,
      employeeId: this.employeeId || undefined,
      assignedStationId: this.assignedStationId || undefined,
      assignedTrainId: this.assignedTrainId || undefined
    };

    this.authService.register(request).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.error || 'Registration failed. Please try again.';
      }
    });
  }
}
