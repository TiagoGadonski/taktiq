type LogLevel = 'debug' | 'info' | 'warn' | 'error';

interface LoggerConfig {
  level: LogLevel;
  enableConsole: boolean;
  enableRemote: boolean;
  remoteEndpoint?: string;
}

const LOG_LEVELS: Record<LogLevel, number> = {
  debug: 0,
  info: 1,
  warn: 2,
  error: 3,
};

class Logger {
  private config: LoggerConfig;

  constructor(config: Partial<LoggerConfig> = {}) {
    this.config = {
      level: config.level || 'info',
      enableConsole: config.enableConsole ?? true,
      enableRemote: config.enableRemote ?? false,
      remoteEndpoint: config.remoteEndpoint,
    };
  }

  private shouldLog(level: LogLevel): boolean {
    return LOG_LEVELS[level] >= LOG_LEVELS[this.config.level];
  }

  private formatMessage(level: LogLevel, message: string, meta?: any): string {
    const timestamp = new Date().toISOString();
    const metaStr = meta ? ` ${JSON.stringify(meta)}` : '';
    return `[${timestamp}] [${level.toUpperCase()}] ${message}${metaStr}`;
  }

  private async sendToRemote(level: LogLevel, message: string, meta?: any): Promise<void> {
    if (!this.config.enableRemote || !this.config.remoteEndpoint) {
      return;
    }

    try {
      await fetch(this.config.remoteEndpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          level,
          message,
          meta,
          timestamp: new Date().toISOString(),
        }),
      });
    } catch (error) {
      // Fail silently to avoid infinite loops
      console.error('Failed to send log to remote:', error);
    }
  }

  debug(message: string, meta?: any): void {
    if (this.shouldLog('debug')) {
      if (this.config.enableConsole) {
        console.debug(this.formatMessage('debug', message, meta));
      }
      this.sendToRemote('debug', message, meta);
    }
  }

  info(message: string, meta?: any): void {
    if (this.shouldLog('info')) {
      if (this.config.enableConsole) {
        console.info(this.formatMessage('info', message, meta));
      }
      this.sendToRemote('info', message, meta);
    }
  }

  warn(message: string, meta?: any): void {
    if (this.shouldLog('warn')) {
      if (this.config.enableConsole) {
        console.warn(this.formatMessage('warn', message, meta));
      }
      this.sendToRemote('warn', message, meta);
    }
  }

  error(message: string, meta?: any): void {
    if (this.shouldLog('error')) {
      if (this.config.enableConsole) {
        console.error(this.formatMessage('error', message, meta));
      }
      this.sendToRemote('error', message, meta);
    }
  }
}

export const logger = new Logger({
  level: process.env.NODE_ENV === 'production' ? 'warn' : 'debug',
  enableConsole: true,
  enableRemote: false,
});

export { Logger, type LoggerConfig, type LogLevel };
