import { describe, it, expect } from 'vitest';
import { loginSchema, signupSchema } from './auth';

describe('loginSchema', () => {
  it('should validate correct login data', () => {
    const validData = {
      email: 'user@example.com',
      password: 'password123',
    };
    const result = loginSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should reject invalid email', () => {
    const invalidData = {
      email: 'invalid-email',
      password: 'password123',
    };
    const result = loginSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });

  it('should reject short password', () => {
    const invalidData = {
      email: 'user@example.com',
      password: '12345',
    };
    const result = loginSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });
});

describe('signupSchema', () => {
  it('should validate correct signup data', () => {
    const validData = {
      email: 'user@example.com',
      password: 'Password123',
      name: 'John Doe',
    };
    const result = signupSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should reject password without uppercase', () => {
    const invalidData = {
      email: 'user@example.com',
      password: 'password123',
      name: 'John Doe',
    };
    const result = signupSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });

  it('should reject password without number', () => {
    const invalidData = {
      email: 'user@example.com',
      password: 'PasswordABC',
      name: 'John Doe',
    };
    const result = signupSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });

  it('should reject short name', () => {
    const invalidData = {
      email: 'user@example.com',
      password: 'Password123',
      name: 'J',
    };
    const result = signupSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });
});
