type LogLevel = "debug" | "info" | "warn" | "error";

type LogMeta = Record<string, unknown>;

type SerializedError = {
  name: string;
  message: string;
  stack?: string;
};

function serializeError(error: unknown): SerializedError | undefined {
  if (!error) {
    return undefined;
  }

  if (error instanceof Error) {
    return {
      name: error.name,
      message: error.message,
      stack: error.stack
    };
  }

  return {
    name: "Error",
    message: String(error)
  };
}

function write(level: LogLevel, message: string, meta?: LogMeta, error?: unknown): void {
  const entry: Record<string, unknown> = {
    level,
    message,
    time: new Date().toISOString()
  };

  if (meta) {
    Object.assign(entry, meta);
  }

  const serialized = serializeError(error);
  if (serialized) {
    entry.error = serialized;
  }

  console.log(JSON.stringify(entry));
}

export const logger = {
  debug(message: string, meta?: LogMeta): void {
    write("debug", message, meta);
  },
  info(message: string, meta?: LogMeta): void {
    write("info", message, meta);
  },
  warn(message: string, meta?: LogMeta): void {
    write("warn", message, meta);
  },
  error(message: string, error?: unknown, meta?: LogMeta): void {
    write("error", message, meta, error);
  }
};
