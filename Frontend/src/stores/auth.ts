import { ref } from 'vue'
import { defineStore } from 'pinia'
import api from '../services/api'
import type { LoginRequest, LoginResponse } from '../types/authType'

export const useAuthStore = defineStore('auth', () => {
    const accessToken = ref<string | null>(
        localStorage.getItem('accessToken')
    )
    const refreshToken = ref<string | null>(
        localStorage.getItem('refreshToken')
    )
    const userId = ref<string | null>(
        localStorage.getItem('userId')
    )
    const email = ref<string | null>(
        localStorage.getItem('email')
    )
    const storedRole = localStorage.getItem('role')
    
    const role = ref<number | null>(
        storedRole ? Number(storedRole) : null
    )

    const isAuthenticated = ref(!!accessToken.value)

    async function login(request: LoginRequest) {
        const response = await api.post<LoginResponse>('/api/auth/login', request)

        const data = response.data

        accessToken.value = data.accessToken
        refreshToken.value = data.refreshToken
        userId.value = data.userId
        email.value = data.email
        role.value = data.role

        localStorage.setItem('accessToken', data.accessToken)
        localStorage.setItem('refreshToken', data.refreshToken)
        localStorage.setItem('userId', data.userId)
        localStorage.setItem('email', data.email)
        localStorage.setItem('role', String(data.role))

        isAuthenticated.value = true
    }

    async function logout() {
        accessToken.value = null
        refreshToken.value = null
        userId.value = null
        email.value = null
        role.value = null

        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
        localStorage.removeItem('userId')
        localStorage.removeItem('email')
        localStorage.removeItem('role')

        isAuthenticated.value = false
    }

    return{
        accessToken,
        refreshToken,
        userId,
        email,
        role,
        isAuthenticated,
        login,
        logout,
    }
})