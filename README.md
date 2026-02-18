# OptimusFrame.Core.API

Micro serviço Core do fluxo de upload e envio de vídeos para AWS S3.

## 🚀 CI/CD Pipeline

A pipeline automatiza build, testes, criação de imagens Docker e deploy no AWS EKS.

### Fluxo de Trabalho

- **Push/PR**: Executa build, testes e criação de imagem Docker
- **Push (main/develop)**: Publica imagem no GitHub Container Registry com assinatura Cosign
- **Deploy Manual**: Atualiza cluster Kubernetes via workflow_dispatch

### Nomenclatura de Imagens

```
ghcr.io/<owner>/prod-optimusframe-auth:<commit-hash>  # main
ghcr.io/<owner>/dev-optimusframe-auth:<commit-hash>   # develop
```

### Secrets Necessários

Configure em **Settings → Secrets and variables → Actions**:

| Secret | Descrição | Exemplo |
|--------|-----------|---------|
| `AWS_ACCESS_KEY_ID` | AWS Access Key ID | `AKIAIOSFODNN7EXAMPLE` |
| `AWS_SECRET_ACCESS_KEY` | AWS Secret Access Key | `wJalrXUt...` |
| `AWS_REGION` | Região do cluster EKS | `us-east-1` |
| `EKS_CLUSTER_NAME` | Nome do cluster EKS | `optimus-frame-cluster` |

### Permissões IAM AWS

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["eks:DescribeCluster", "eks:ListClusters"],
      "Resource": "*"
    }
  ]
}
```

### Deploy Manual

1. Acesse **Actions** → **OptimusFrame Core CI/CD**
2. Clique em **Run workflow**
3. Selecione ambiente: `develop` ou `production`
4. Confirme execução

## 🔍 SonarQube Analysis

Análise automática de qualidade de código no SonarCloud.

### Triggers

- Push em `main`, `develop` ou `master`
- Pull requests
- Execução manual

### Secrets Necessários

| Secret | Descrição | Onde Obter |
|--------|-----------|------------|
| `SONAR_TOKEN` | Token de autenticação | SonarCloud → Account → Security |
| `SONAR_PROJECT_KEY` | Chave do projeto | SonarCloud → Project Settings |

### Configuração Inicial

1. Acesse [SonarCloud](https://sonarcloud.io)
2. Importe o repositório GitHub
3. Copie o **Project Key**
4. Gere um **Token** em Account → Security
5. Adicione secrets no GitHub
