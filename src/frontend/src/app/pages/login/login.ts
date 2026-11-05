import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface DemoAccount {
  role: string;
  email: string;
  password: string;
  description: string;
}

const DEFAULT_DEMO_ACCOUNTS: DemoAccount[] = [
  {
    role: 'RH Geral',
    email: 'admin@dndgroup.com',
    password: 'Password123!',
    description: 'Acesso total, relatórios e fechamentos'
  },
  {
    role: 'Gestor (Brasil)',
    email: 'gestor.brasil@dndgroup.com',
    password: 'Password123!',
    description: 'Gestão da equipe BR e aprovações'
  },
  {
    role: 'Gestor (Europa)',
    email: 'gestor.europa@dndgroup.com',
    password: 'Password123!',
    description: 'Gestão da equipe EU e aprovações'
  },
  {
    role: 'Colaborador (Brasil)',
    email: 'colab.brasil@dndgroup.com',
    password: 'Password123!',
    description: 'Horário de Brasília (UTC-3)'
  },
  {
    role: 'Colaborador (Viajante)',
    email: 'colab.viajante@dndgroup.com',
    password: 'Password123!',
    description: 'Viagens frequentes entre BR e Portugal (UTC-3 / UTC+0)'
  },
  {
    role: 'Colaborador (Europa)',
    email: 'colab.europa@dndgroup.com',
    password: 'Password123!',
    description: 'Horário de Lisboa (UTC+0 / UTC+1)'
  }
];

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent implements OnInit {
  email = '';
  password = '';
  errorMessage = signal<string | null>(null);
  isLoading = signal(false);
  demoAccounts = signal<DemoAccount[]>(DEFAULT_DEMO_ACCOUNTS);

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/ponto']);
    }

    this.authService.getDemoAccounts().subscribe({
      next: (accounts) => {
        if (accounts && accounts.length > 0) {
          this.demoAccounts.set(accounts);
        }
      },
      error: () => {
        // Mantém as contas padrão de fallback caso a API ainda esteja iniciando
      }
    });
  }

  onSubmit(): void {
    if (!this.email || !this.password) {
      this.errorMessage.set('Por favor, preencha o e-mail e a senha.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/ponto']);
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.status === 0) {
          this.errorMessage.set(
            '⚠️ Servidor Backend não encontrado em http://localhost:5000. Certifique-se de que o backend (.NET 8) foi iniciado no terminal com ./run-backend.sh!'
          );
        } else {
          this.errorMessage.set(err.error?.message || 'Falha ao autenticar. Verifique suas credenciais.');
        }
      }
    });
  }

  quickLogin(account: DemoAccount): void {
    this.email = account.email;
    this.password = account.password;
    this.onSubmit();
  }
}
