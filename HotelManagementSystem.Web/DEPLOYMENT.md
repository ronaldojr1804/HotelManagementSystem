# Guia de Implantação e Configuração

Este guia descreve como implantar o Hotel Management System em ambientes Windows e Linux, e como configurar diferentes bancos de dados.

## Pré-requisitos

- **.NET 8 SDK** (para compilação) ou **.NET 8 Runtime** (para execução).
- Banco de dados (SQLite, PostgreSQL ou MySQL) instalado e acessível (exceto SQLite que é baseado em arquivo).

---

## 1. Publicação da Aplicação

Antes de implantar, é necessário publicar a aplicação para gerar os arquivos otimizados.

Abra o terminal na pasta do projeto (`HotelManagementSystem.Web`) e execute:

```bash
dotnet publish -c Release -o ./publish
```

Isso criará uma pasta `publish` com todos os arquivos necessários.

---

## 2. Implantação no Windows

### Opção A: Executando como Serviço do Windows (IIS)

1.  Instale o **.NET Core Hosting Bundle** no servidor.
2.  No IIS Manager, crie um novo Site.
3.  Aponte o caminho físico para a pasta onde você copiou os arquivos publicados (ex: `C:\inetpub\wwwroot\hotel-sys`).
4.  Configure o Pool de Aplicativos para usar "No Managed Code".

### Opção B: Executando Manualmente (Kestrel)

1.  Copie a pasta `publish` para o servidor.
2.  Abra o PowerShell ou CMD na pasta.
3.  Execute:
    ```cmd
    dotnet HotelManagementSystem.Web.dll
    ```
4.  Acesse `http://localhost:5000` (ou a porta configurada).

---

## 3. Implantação no Linux (Ubuntu/Debian)

### Passo 1: Instalar o .NET Runtime

```bash
sudo apt-get update && \
  sudo apt-get install -y dotnet-runtime-8.0 aspnetcore-runtime-8.0
```

### Passo 2: Configurar o Serviço (Systemd)

1.  Copie os arquivos publicados para `/var/www/hotel-sys`.
2.  Crie um arquivo de serviço: `sudo nano /etc/systemd/system/kestrel-hotel.service`

```ini
[Unit]
Description=Hotel Management System

[Service]
WorkingDirectory=/var/www/hotel-sys
ExecStart=/usr/bin/dotnet /var/www/hotel-sys/HotelManagementSystem.Web.dll
Restart=always
# Reiniciar serviço após 10 segundos se falhar
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-hotel
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

3.  Ative e inicie o serviço:

```bash
sudo systemctl enable kestrel-hotel.service
sudo systemctl start kestrel-hotel.service
```

### Passo 3: Configurar Proxy Reverso (Nginx)

Recomendado para expor a aplicação na porta 80/443.

1.  Instale o Nginx: `sudo apt install nginx`
2.  Edite a configuração: `sudo nano /etc/nginx/sites-available/default`

```nginx
server {
    listen 80;
    server_name seu-dominio.com;

    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

3.  Reinicie o Nginx: `sudo systemctl restart nginx`

---

## 4. Configuração de Banco de Dados

A aplicação suporta SQLite, PostgreSQL e MySQL. A configuração é feita no arquivo `appsettings.json`.

### SQLite (Padrão)
Ideal para testes e pequenas implantações.

```json
"DatabaseProvider": "Sqlite",
"ConnectionStrings": {
  "DefaultConnection": "Data Source=hotel.db"
}
```

### PostgreSQL
Recomendado para produção.

1.  Altere o `DatabaseProvider` para `Postgres`.
2.  Ajuste a string de conexão.

```json
"DatabaseProvider": "Postgres",
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=hotel_db;Username=seu_usuario;Password=sua_senha"
}
```

# Guia de Implantação e Configuração

Este guia descreve como implantar o Hotel Management System em ambientes Windows e Linux, e como configurar diferentes bancos de dados.

## Pré-requisitos

- **.NET 8 SDK** (para compilação) ou **.NET 8 Runtime** (para execução).
- Banco de dados (SQLite, PostgreSQL ou MySQL) instalado e acessível (exceto SQLite que é baseado em arquivo).

---

## 1. Publicação da Aplicação

Antes de implantar, é necessário publicar a aplicação para gerar os arquivos otimizados.

Abra o terminal na pasta do projeto (`HotelManagementSystem.Web`) e execute:

```bash
dotnet publish -c Release -o ./publish
```

Isso criará uma pasta `publish` com todos os arquivos necessários.

---

## 2. Implantação no Windows

### Opção A: Executando como Serviço do Windows (IIS)

1.  Instale o **.NET Core Hosting Bundle** no servidor.
2.  No IIS Manager, crie um novo Site.
3.  Aponte o caminho físico para a pasta onde você copiou os arquivos publicados (ex: `C:\inetpub\wwwroot\hotel-sys`).
4.  Configure o Pool de Aplicativos para usar "No Managed Code".

### Opção B: Executando Manualmente (Kestrel)

1.  Copie a pasta `publish` para o servidor.
2.  Abra o PowerShell ou CMD na pasta.
3.  Execute:
    ```cmd
    dotnet HotelManagementSystem.Web.dll
    ```
4.  Acesse `http://localhost:5000` (ou a porta configurada).

---

## 3. Implantação no Linux (Ubuntu/Debian)

### Passo 1: Instalar o .NET Runtime

```bash
sudo apt-get update && \
  sudo apt-get install -y dotnet-runtime-8.0 aspnetcore-runtime-8.0
```

### Passo 2: Configurar o Serviço (Systemd)

1.  Copie os arquivos publicados para `/var/www/hotel-sys`.
2.  Crie um arquivo de serviço: `sudo nano /etc/systemd/system/kestrel-hotel.service`

```ini
[Unit]
Description=Hotel Management System

[Service]
WorkingDirectory=/var/www/hotel-sys
ExecStart=/usr/bin/dotnet /var/www/hotel-sys/HotelManagementSystem.Web.dll
Restart=always
# Reiniciar serviço após 10 segundos se falhar
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-hotel
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

3.  Ative e inicie o serviço:

```bash
sudo systemctl enable kestrel-hotel.service
sudo systemctl start kestrel-hotel.service
```

### Passo 3: Configurar Proxy Reverso (Nginx)

Recomendado para expor a aplicação na porta 80/443.

1.  Instale o Nginx: `sudo apt install nginx`
2.  Edite a configuração: `sudo nano /etc/nginx/sites-available/default`

```nginx
server {
    listen 80;
    server_name seu-dominio.com;

    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

3.  Reinicie o Nginx: `sudo systemctl restart nginx`

---

## 4. Configuração de Banco de Dados

A aplicação suporta SQLite, PostgreSQL e MySQL. A configuração é feita no arquivo `appsettings.json`.

### SQLite (Padrão)
Ideal para testes e pequenas implantações.

```json
"DatabaseProvider": "Sqlite",
"ConnectionStrings": {
  "DefaultConnection": "Data Source=hotel.db"
}
```

### PostgreSQL
Recomendado para produção.

1.  Altere o `DatabaseProvider` para `Postgres`.
2.  Ajuste a string de conexão.

```json
"DatabaseProvider": "Postgres",
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=hotel_db;Username=seu_usuario;Password=sua_senha"
}
```

### MySQL
Alternativa robusta.

1.  Altere o `DatabaseProvider` para `MySql`.
2.  Ajuste a string de conexão.

```json
"DatabaseProvider": "MySql",
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=hotel_db;User=seu_usuario;Password=sua_senha;"
}
```

### Aplicando Migrações

A aplicação foi configurada para **aplicar migrações automaticamente** na inicialização, tanto em desenvolvimento quanto em produção. Isso significa que ao iniciar a aplicação pela primeira vez (ou após uma atualização), o banco de dados será criado ou atualizado automaticamente.

Caso prefira aplicar as migrações manualmente (por exemplo, em um pipeline de CI/CD ou para ter mais controle), você pode usar o comando:

```bash
dotnet ef database update
```

### Configuração de Autenticação

A aplicação suporta autenticação com o Identity do ASP.NET Core. A configuração é feita no arquivo `appsettings.json`.

```json
"Authentication": {
  "Cookie": {
    "LoginPath": "/login",
    "LogoutPath": "/logout"
  }
}
```

## Apenas para teste Usuario padrão: Email: admin@hotel.com Password: admin