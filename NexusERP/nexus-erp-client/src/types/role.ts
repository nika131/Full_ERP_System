export interface RoleResponse {
    roleId: number;
    name: string;
    permissions: string[];
    isSystemRole?: boolean;
}

export interface RoleLookup {
    roleId: number;
    name: string;
}
