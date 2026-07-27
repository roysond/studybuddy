const DEFAULT_API_BASE_URL = 'http://localhost:5017';

/**
 * Single source of truth for the StudyBuddy API base URL.
 * Override locally with VITE_API_BASE_URL in a .env file.
 */
export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL?.trim() || DEFAULT_API_BASE_URL;
