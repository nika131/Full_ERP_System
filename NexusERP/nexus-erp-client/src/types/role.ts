export interface RoleResponse {
    roleId: number;
    name: string;
    permissions: string[];
}

export interface RoleLookup {
    roleId: number;
    name: string;
}