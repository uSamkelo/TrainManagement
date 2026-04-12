import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { UserDTO } from '../../interfaces/IAuth';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-users.component.html',
  styleUrls: ['./admin-users.component.css']
})
export class AdminUsersComponent implements OnInit {
  users: UserDTO[] = [];
  loading = true;
  error = '';
  filterRole = '';
  searchTerm = '';

  roles = ['Passenger', 'Driver', 'StationStaff', 'Admin'];
  roleColors: { [key: string]: string } = {
    Passenger: '#3498db',
    Driver: '#e67e22',
    StationStaff: '#27ae60',
    Admin: '#e74c3c'
  };
  roleIcons: { [key: string]: string } = {
    Passenger: '🧑‍💼',
    Driver: '🚂',
    StationStaff: '🎫',
    Admin: '🛡️'
  };

  constructor(private userService: UserService, public auth: AuthService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.userService.getAll().subscribe({
      next: (users) => { this.users = users; this.loading = false; },
      error: () => { this.loading = false; this.error = 'Failed to load users.'; }
    });
  }

  get filteredUsers(): UserDTO[] {
    let result = this.users;
    if (this.filterRole) result = result.filter(u => u.role === this.filterRole);
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      result = result.filter(u =>
        u.firstName.toLowerCase().includes(term) ||
        u.lastName.toLowerCase().includes(term) ||
        u.email.toLowerCase().includes(term)
      );
    }
    return result;
  }

  getRoleCount(role: string): number {
    return this.users.filter(u => u.role === role).length;
  }

  deactivateUser(user: UserDTO): void {
    if (!confirm(`Deactivate ${user.firstName} ${user.lastName}?`)) return;
    this.userService.deactivate(user.id).subscribe({
      next: () => this.loadUsers(),
      error: () => this.error = 'Failed to deactivate user.'
    });
  }
}
