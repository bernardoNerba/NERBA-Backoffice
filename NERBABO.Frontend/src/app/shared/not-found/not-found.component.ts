import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  templateUrl: './not-found.component.html',
})
export class NotFoundComponent implements OnInit {
  constructor(private authService: AuthService) {}
  isAuthenticated: boolean = false;
  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated;
  }
}
