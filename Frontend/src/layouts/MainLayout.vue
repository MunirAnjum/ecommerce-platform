<script setup lang="ts">
import Navbar from '../components/Navbar.vue'
import Footer from '../components/Footer.vue'
</script>

<template>
  <div class="flex min-h-screen flex-col bg-[#FDFBF7] text-stone-900 selection:bg-amber-200 selection:text-stone-900">
    <Navbar />

    <main class="flex-1">
      <router-view />
    </main>

    <Footer />
  </div>
</template>