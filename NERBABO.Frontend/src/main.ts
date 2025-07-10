import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { importProvidersFrom } from '@angular/core';
import { ModalModule } from 'ngx-bootstrap/modal';
import { authInterceptor } from './app/shared/interceptors/auth.interceptor';
import { routes } from './app/app.routes';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { definePreset } from '@primeng/themes';

// Define custom light green theme based on Aura
const MyLightGreenTheme = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#e8f5e8',
      100: '#c3e6c3',
      200: '#9ed69e',
      300: '#78c578',
      400: '#53b553',
      500: '#009241', // primary green
      600: '#007838',
      700: '#005d2e',
      800: '#004325',
      900: '#002a17',
      950: '#015f2b', // dark green
    },
  },
});

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimations(),
    importProvidersFrom(ModalModule.forRoot()),
    providePrimeNG({
      theme: {
        preset: MyLightGreenTheme,
        options: {
          prefix: 'p',
          darkModeSelector: false,
          cssLayer: false,
        },
      },
    }),
  ],
}).catch((err) => console.error(err));
