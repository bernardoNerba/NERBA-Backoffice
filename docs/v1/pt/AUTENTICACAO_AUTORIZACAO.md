# Autenticacao e Autorizacao

Este documento descreve o sistema de autenticacao JWT e autorizacao implementado no NERBA Backoffice, incluindo validacoes, regras de negocio, componentes e fluxos de seguranca.

## Indice

- [Visao Geral](#visao-geral)
- [Validacoes de Autenticacao](#validacoes-de-autenticacao)
- [Regras de Negocio de Autenticacao](#regras-de-negocio-de-autenticacao)
- [Autorizacao (Roles e Politicas)](#autorizacao-roles-e-politicas)
- [Componentes do Sistema](#componentes-do-sistema)
- [Fluxos de Autenticacao](#fluxos-de-autenticacao)
- [Seguranca](#seguranca)
- [Endpoints da API](#endpoints-da-api)
- [Estrutura do JWT](#estrutura-do-jwt)
- [Referencias](#referencias)

---

## Visao Geral

O NERBA Backoffice implementa autenticacao baseada em JWT (JSON Web Token) para comunicacao segura entre o frontend Angular e o backend ASP.NET Core.

### Stack Tecnologica

| Camada | Tecnologia | Versao |
|--------|------------|--------|
| Frontend | Angular (Standalone Components) | 19 |
| Backend | ASP.NET Core | 8+ |
| Biblioteca JWT (Frontend) | jwt-decode | - |
| Armazenamento (Frontend) | LocalStorage | - |
| Cache (Backend) | Redis Distributed Cache | - |
| Gestao de Estado | RxJS BehaviorSubject | - |

### Arquitectura de Alto Nivel

```
+-------------+         +--------------+         +-------------+
|   Angular   |  HTTP   |   ASP.NET    |  Redis  |   Token     |
|   Frontend  | <-----> |   Backend    | <-----> |  Blacklist  |
+-------------+         +--------------+         +-------------+
      |                        |
      | LocalStorage           | JWT Validation
      | (NerbaBackofficeUser)  | Token Blacklist Check
```

---

## Validacoes de Autenticacao

### LoginDto (Backend)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `UsernameOrEmail` | Obrigatorio | Validacao padrao DataAnnotations |
| `Password` | Obrigatorio | Validacao padrao DataAnnotations |

### UserRoleDto (Backend)

| Campo | Validacoes | Mensagem de Erro |
|-------|------------|------------------|
| `Roles` | Obrigatorio, minimo 1 item | "Pelo menos 1 papel e obrigatorio." |
| `UserId` | Obrigatorio | Validacao padrao DataAnnotations |

### Login Model (Frontend)

| Campo | Validacoes | Comportamento |
|-------|------------|---------------|
| `usernameOrEmail` | Obrigatorio | Formulario invalido se vazio |
| `password` | Obrigatorio | Formulario invalido se vazio |

### Validacao de Token (Frontend)

| Validacao | Descricao | Comportamento |
|-----------|-----------|---------------|
| Formato JWT | Token deve ter 3 partes separadas por `.` | Erro de descodificacao |
| Expiracao | Claim `exp` validada com buffer de 5 segundos | Token considerado expirado |
| Claims obrigatorias | `nameid`, `email`, `role`, `exp` | Erro de descodificacao |

---

## Regras de Negocio de Autenticacao

### JWT Token

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Expiracao configuravel | Tokens expiram apos X dias (configuracao `JWT:ExpiresInDays`) | Backend |
| Claims padrao | `nameid`, `email`, `given_name`, `family_name`, `role` | Backend |
| Algoritmo | HMAC SHA256 Signature | Backend |
| Clock skew | 5 segundos de tolerancia para expiracao | Frontend |
| Issuer | Configurado em `JWT:Issuer` | Backend |

### Token Refresh

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Endpoint | GET `/api/auth/refresh-user-token` | Backend |
| Autenticacao | Requer token valido (Bearer) | Backend |
| Politica | Requer utilizador ativo (`ActiveUser`) | Backend |
| Prevencao duplicados | Flag `isRefreshing` impede multiplas chamadas | Frontend |
| Retry automatico | Request original repetido apos refresh bem-sucedido | Frontend |

### Logout e Invalidacao

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Blacklist Redis | Token adicionado a blacklist com chave `blacklist:token:{token}` | Backend |
| TTL automatico | Token removido da blacklist apos expiracao natural | Backend |
| Fail-secure | Em caso de erro Redis, acesso negado | Backend |
| Client-side | LocalStorage limpo imediatamente | Frontend |
| Fire-and-forget | Logout client-side nao espera resposta do servidor | Frontend |

### Login

| Regra | Descricao | Camada |
|-------|-----------|--------|
| Autenticacao flexivel | Aceita username ou email | Backend |
| Utilizador bloqueado | Retorna 401 se `IsActive = false` | Backend |
| Ultimo login | Sistema actualiza `LastLogin` automaticamente | Backend |
| Armazenamento | User object (com JWT) guardado em LocalStorage | Frontend |

---

## Autorizacao (Roles e Politicas)

### Roles Disponiveis

| Role | Descricao | Permissoes |
|------|-----------|------------|
| `Admin` | Administrador do sistema | Acesso total, gestao de utilizadores e roles |
| `User` | Utilizador basico | Acesso a funcionalidades padrao |
| `CQ` | Coordenador de Qualidade | Gestao de qualidade de formacao |
| `FM` | Gestor Financeiro | Gestao financeira e pagamentos |

### Politicas

| Politica | Descricao | Implementacao |
|----------|-----------|---------------|
| `ActiveUser` | Requer utilizador com `IsActive = true` | `ActiveUserHandler` + `ActiveUserRequirement` |

### Verificacao de Roles (Frontend)

| Propriedade | Descricao | Logica |
|-------------|-----------|--------|
| `userRoles` | Array de roles do utilizador | Extraido do claim `role` do JWT |
| `isUserAdmin` | Indica se utilizador e Admin | `role.includes('Admin')` |

---

## Componentes do Sistema

### Frontend (Angular)

| Componente | Ficheiro | Responsabilidade |
|------------|----------|------------------|
| AuthService | `core/services/auth.service.ts` | Gestao de autenticacao, tokens e estado |
| AuthGuard | `shared/guards/auth.guard.ts` | Protecao de rotas autenticadas |
| UnauthOnlyGuard | `shared/guards/unauth-only.guard.ts` | Protecao de rotas so para nao autenticados |
| AuthInterceptor | `shared/interceptors/auth.interceptor.ts` | Injeccao automatica de Bearer token |
| TokenRefreshInterceptor | `shared/interceptors/token-refresh.interceptor.ts` | Renovacao automatica em 401 |
| User Model | `core/models/user.ts` | Modelo do utilizador autenticado |
| Login Model | `core/models/login.ts` | Modelo de credenciais de login |
| JwtPayload Model | `core/models/jwtPayload.ts` | Estrutura das claims JWT |

### Backend (ASP.NET Core)

| Componente | Ficheiro | Responsabilidade |
|------------|----------|------------------|
| AuthController | `Core/Authentication/Controllers/AuthController.cs` | Endpoints de autenticacao |
| JwtService | `Core/Authentication/Services/JwtService.cs` | Geracao e validacao de tokens |
| TokenBlacklistService | `Core/Authentication/Services/TokenBlacklistService.cs` | Gestao de blacklist Redis |
| TokenBlacklistMiddleware | `Shared/Middleware/TokenBlacklistMiddleware.cs` | Verificacao de blacklist em requests |
| ActiveUserHandler | `Core/Authentication/Models/ActiveUserHandler.cs` | Handler da politica ActiveUser |
| LoginDto | `Core/Authentication/Dtos/LoginDto.cs` | DTO de login |
| LoggedInUserDto | `Core/Authentication/Dtos/LoggedInUserDto.cs` | DTO de resposta de login |
| UserRoleDto | `Core/Authentication/Dtos/UserRoleDto.cs` | DTO para atribuicao de roles |

---

## Fluxos de Autenticacao

### Login Flow

```
1. Utilizador submete credenciais (usernameOrEmail + password)
   |
2. Frontend: POST /api/auth/login
   |
3. Backend: Valida credenciais
   |-- Utilizador nao encontrado -> 400 Bad Request
   |-- Utilizador bloqueado -> 401 Unauthorized
   |-- Password invalida -> 400 Bad Request
   |
4. Backend: Gera JWT com claims
   |
5. Backend: Actualiza LastLogin
   |
6. Frontend: Armazena User em LocalStorage
   |
7. Frontend: Emite novo estado via BehaviorSubject
   |
8. Frontend: Redireciona para dashboard (ou returnUrl)
```

### Token Refresh Flow

```
1. Request retorna 401 Unauthorized
   |
2. TokenRefreshInterceptor captura erro
   |-- E endpoint de auth? -> Propaga erro
   |
3. Verifica flag isRefreshing
   |-- Ja a refrescar? -> Retorna
   |
4. GET /api/auth/refresh-user-token
   |
5. Backend: Valida token actual
   |-- Token invalido -> 401 (logout no frontend)
   |
6. Backend: Gera novo JWT
   |
7. Frontend: Armazena novo token
   |
8. Frontend: Repete request original com novo token
```

### Logout Flow

```
1. Utilizador clica logout
   |
2. Frontend: POST /api/auth/logout (fire-and-forget)
   |
3. Backend: Extrai token do header
   |
4. Backend: Descodifica token para obter expiracao
   |
5. Backend: Adiciona a blacklist Redis
   |-- Chave: blacklist:token:{jwt}
   |-- TTL: Ate expiracao natural do token
   |
6. Frontend: Limpa LocalStorage (imediato)
   |
7. Frontend: Emite null via BehaviorSubject
   |
8. Frontend: Redireciona para login
```

### Aplicacao Load Flow

```
1. Aplicacao inicia
   |
2. AuthService: loadUserFromStorage()
   |
3. Le LocalStorage (NerbaBackofficeUser)
   |-- Sem dados? -> Emite null
   |
4. Valida expiracao do token
   |-- Expirado? -> removeUser() + Emite null
   |
5. Token valido -> Emite user via BehaviorSubject
```

---

## Seguranca

### Medidas Implementadas

| Medida | Descricao | Estado |
|--------|-----------|--------|
| JWT stateless | Autenticacao escalavel sem sessoes servidor | Implementado |
| Server-side logout | Blacklist em Redis para invalidacao | Implementado |
| Token refresh automatico | Renovacao transparente em 401 | Implementado |
| Route guards | Proteccao de rotas Angular | Implementado |
| Bearer token injection | Interceptor automatico | Implementado |
| Clock skew tolerance | 5 segundos de tolerancia | Implementado |
| Duplicate refresh prevention | Flag isRefreshing | Implementado |
| Fail-secure blacklist | Nega acesso em erros Redis | Implementado |
| Politica ActiveUser | Verifica utilizador activo em BD | Implementado |

### Vulnerabilidades Conhecidas e Mitigacoes

| Vulnerabilidade | Risco | Mitigacao Recomendada | Estado |
|-----------------|-------|----------------------|--------|
| LocalStorage (XSS) | Alto | Migrar para HttpOnly cookies | Pendente |
| Console logs em producao | Baixo | Remover ou condicionar por ambiente | Pendente |
| CSRF | Medio | Implementar tokens CSRF ou SameSite cookies | Pendente |
| Token sem validacao de formato | Baixo | Validar estrutura antes de guardar | Pendente |
| Hardcoded role names | Baixo | Centralizar em enum/constantes | Pendente |

### Recomendacoes Futuras

| Recomendacao | Prioridade | Descricao |
|--------------|------------|-----------|
| HttpOnly Cookies | Alta | Armazenar JWT em cookies seguros |
| Refresh Token Pattern | Media | Access token curto + refresh token longo |
| Session Timeout Warning | Baixa | Avisar utilizador antes de expiracao |
| Role-based Guards | Media | Guards especificos por role |
| Security Headers | Media | X-Content-Type-Options, X-Frame-Options, CSP |

---

## Endpoints da API

| Metodo | Endpoint | Descricao | Autenticacao | Autorizacao |
|--------|----------|-----------|--------------|-------------|
| POST | `/api/auth/login` | Autenticacao de utilizador | Nao | - |
| POST | `/api/auth/logout` | Logout e invalidacao de token | Sim (Bearer) | ActiveUser |
| GET | `/api/auth/refresh-user-token` | Renovar token JWT | Sim (Bearer) | ActiveUser |
| POST | `/api/auth/set-role` | Atribuir roles a utilizador | Sim (Bearer) | Admin + ActiveUser |

### Respostas HTTP

| Codigo | Descricao | Cenarios |
|--------|-----------|----------|
| 200 | Sucesso | Login/logout/refresh bem-sucedido |
| 400 | Bad Request | Credenciais invalidas, validacao falhou |
| 401 | Unauthorized | Token invalido/expirado, utilizador bloqueado, token na blacklist |
| 404 | Not Found | Utilizador nao encontrado (refresh) |
| 500 | Internal Error | Erro inesperado |

---

## Estrutura do JWT

### Claims Padrao

| Claim | Tipo .NET | Descricao | Exemplo |
|-------|-----------|-----------|---------|
| `nameid` | ClaimTypes.NameIdentifier | ID unico do utilizador | `"guid-xxx-xxx"` |
| `email` | ClaimTypes.Email | Email do utilizador | `"user@example.com"` |
| `given_name` | ClaimTypes.GivenName | Primeiro nome | `"Joao"` |
| `family_name` | ClaimTypes.Surname | Ultimo nome | `"Silva"` |
| `role` | ClaimTypes.Role | Array de roles | `["Admin", "User"]` |
| `exp` | - | Unix timestamp de expiracao | `1735234567` |

### Estrutura Frontend (JwtPayload)

```typescript
interface JwtPayload {
  nameid: string;        // ID do utilizador
  email: string;         // Email
  given_name: string;    // Primeiro nome
  family_name: string;   // Ultimo nome
  role: Array<string>;   // Roles (pode ser string se apenas 1)
}
```

### Estrutura User (Frontend)

```typescript
type User = {
  firstName: string;
  lastName: string;
  jwt: string;
};
```

---

## Referencias

### Ficheiros de Codigo Fonte

**Backend:**
- `NERBABO.Backend/NERBABO.ApiService/Core/Authentication/Controllers/AuthController.cs`
- `NERBABO.Backend/NERBABO.ApiService/Core/Authentication/Services/JwtService.cs`
- `NERBABO.Backend/NERBABO.ApiService/Core/Authentication/Services/TokenBlacklistService.cs`
- `NERBABO.Backend/NERBABO.ApiService/Shared/Middleware/TokenBlacklistMiddleware.cs`
- `NERBABO.Backend/NERBABO.ApiService/Core/Authentication/Models/ActiveUserHandler.cs`
- `NERBABO.Backend/NERBABO.ApiService/Core/Authentication/Dtos/`

**Frontend:**
- `NERBABO.Frontend/src/app/core/services/auth.service.ts`
- `NERBABO.Frontend/src/app/shared/guards/auth.guard.ts`
- `NERBABO.Frontend/src/app/shared/guards/unauth-only.guard.ts`
- `NERBABO.Frontend/src/app/shared/interceptors/auth.interceptor.ts`
- `NERBABO.Frontend/src/app/shared/interceptors/token-refresh.interceptor.ts`
- `NERBABO.Frontend/src/app/core/models/`
- `NERBABO.Frontend/src/app/core/objects/apiEndpoints.ts`

### Recursos Externos

> Ref: [Angular Security Guide](https://angular.dev/best-practices/security)

> Ref: [JWT Best Practices - RFC 8725](https://tools.ietf.org/html/rfc8725)

> Ref: [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

> Ref: [jwt-decode Library](https://github.com/auth0/jwt-decode)

> Ref: [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)

### Documentacao Relacionada

> Ver: [Validacoes de Entidades](./VALIDATIONS.md)

> Ver: [Regras de Negocio](./BUSINESS_LOGIC.md)
