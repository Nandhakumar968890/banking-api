# API Documentation

## BankingAPI REST Endpoints

Base URL: `http://localhost:5000` (Development) or `https://your-domain.com` (Production)

### Validation Endpoints

**POST** `/api/validation/validate` - Validate single transaction
**POST** `/api/validation/validate-batch` - Validate multiple transactions

### Confirmation Endpoints

**POST** `/api/confirmation/process` - Process credit confirmation
**GET** `/api/confirmation/status/{utr}` - Get confirmation status
**POST** `/api/confirmation/process-batch` - Process batch confirmations

### Monitoring Endpoints

**GET** `/health` - Health check
**GET** `/api/monitoring/health` - Detailed health
**GET** `/api/monitoring/audit-logs` - Get audit logs
**GET** `/api/monitoring/statistics` - Get statistics
**POST** `/api/monitoring/clear-audit-logs` - Clear old logs

### Encryption Endpoints

**POST** `/api/encryption/encrypt-rsa` - RSA encryption
**POST** `/api/encryption/decrypt-rsa` - RSA decryption
**POST** `/api/encryption/encrypt-aes-cbc` - AES-CBC encryption
**POST** `/api/encryption/decrypt-aes-cbc` - AES-CBC decryption

See full documentation in repository for detailed examples and request/response models.