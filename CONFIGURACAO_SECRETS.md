# 🔐 Configuração de Secrets - Notifications API

## 🎯 Objetivo

As configurações sensíveis (credenciais RabbitMQ) **NÃO devem estar no código-fonte**.

---

## 📋 Configuração com User Secrets (Recomendado)

### Passo 1: Inicializar

```bash
cd "d:\ProjetoFiap\Mcrossserviços NEW FIAP\fcg-notifications-api"
dotnet user-secrets init
```

### Passo 2: Configurar RabbitMQ

```bash
dotnet user-secrets set "RabbitMQ:HostName" "localhost"
dotnet user-secrets set "RabbitMQ:UserName" "guest"
dotnet user-secrets set "RabbitMQ:Password" "guest"
```

### Passo 3: Executar

```bash
dotnet run
```

---

## 🐳 Docker/Kubernetes (Produção)

**Docker Compose:**
```yaml
services:
  fcg-notifications-api:
    environment:
      - RabbitMQ__HostName=rabbitmq-cluster
      - RabbitMQ__UserName=${RABBITMQ_USER}
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
```

**Kubernetes Secret:**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: notifications-secrets
stringData:
  rabbitmq-password: "senha-segura"
---
apiVersion: apps/v1
kind: Deployment
spec:
  template:
    spec:
      containers:
      - name: api
        env:
        - name: RabbitMQ__Password
          valueFrom:
            secretKeyRef:
              name: notifications-secrets
              key: rabbitmq-password
```

---

## ✅ Checklist

- [ ] Removi credenciais hardcoded
- [ ] `.env` está no `.gitignore`
- [ ] Configurei user secrets (dev)
- [ ] RabbitMQ não usa guest/guest em produção

---

**Versão:** 1.0  
**Data:** 21/10/2026
