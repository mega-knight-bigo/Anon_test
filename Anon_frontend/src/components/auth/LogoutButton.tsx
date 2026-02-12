import { useAuth0 } from "@auth0/auth0-react";
import { LogOut } from "lucide-react";

export const LogoutButton = () => {
  const { logout } = useAuth0();

  return (
    <button
      onClick={() => {
        logout({ logoutParams: { returnTo: window.location.origin } });
      }}
      className="text-muted-foreground hover:text-foreground cursor-pointer"
      title="Log out"
    >
      <LogOut className="h-4 w-4" />
    </button>
  );
};
