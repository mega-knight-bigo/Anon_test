import { useEffect } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { setAccessToken } from "@/lib/api";

/**
 * This component only clears the token on logout.
 * Token fetching is handled by useAuth hook.
 */
export const Auth0TokenSync = () => {
  const { isAuthenticated, isLoading } = useAuth0();

  // Clear token when logged out
  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      setAccessToken(null);
    }
  }, [isAuthenticated, isLoading]);

  return null;
};
