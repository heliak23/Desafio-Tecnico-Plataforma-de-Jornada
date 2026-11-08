import { TestBed } from '@angular/core/testing';
import { ThemeToggleComponent } from './theme-toggle';
import { ThemeService } from '../../services/theme.service';

describe('ThemeToggleComponent', () => {
  let component: ThemeToggleComponent;
  let themeService: ThemeService;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [ThemeToggleComponent],
      providers: [ThemeService]
    }).compileComponents();

    const fixture = TestBed.createComponent(ThemeToggleComponent);
    component = fixture.componentInstance;
    themeService = TestBed.inject(ThemeService);
    fixture.detectChanges();
  });

  afterEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should set theme to dark on button click', () => {
    component.setTheme('dark');
    expect(themeService.currentTheme()).toBe('dark');
  });

  it('should set theme to light on button click', () => {
    component.setTheme('dark');
    expect(themeService.currentTheme()).toBe('dark');

    component.setTheme('light');
    expect(themeService.currentTheme()).toBe('light');
  });
});

