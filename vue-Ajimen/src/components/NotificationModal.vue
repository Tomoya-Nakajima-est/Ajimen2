<template>
<div class="modal-overlay" v-if="visible">
    <div class="modal-content">
        <p class="modal-message">{{ message }}</p>
        <button class="modal-button" @click="close">OK</button>
    </div>
    </div>
</template>



<script setup>
const props = defineProps({
    visible: Boolean,
    message: String
})
const emit = defineEmits(['close'])
const close = () => emit('close')
</script>

<style scoped>
.modal-overlay {
    position: fixed;
    top: 0; left: 0;
    width: 100%; height: 100%;
    background: rgba(0, 0, 0, 0.4);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 2000;
}

.modal-content {
    background-color: #fefaf6;
    border: 2px solid #460125;
    padding: 40px 50px;
    border-radius: 16px;
    text-align: center;
    box-shadow: 0 8px 24px rgba(0,0,0,0.3);
    max-width: 400px;
    width: 90%;
}

.modal-message {
    font-size: 20px;
    color: #460125;
    font-family: 'Georgia', serif;
    margin-bottom: 30px;
}

.modal-button {
    background-color: #460125;
    color: white;
    border: none;
    padding: 12px 24px;
    font-size: 18px;
    border-radius: 8px;
    cursor: pointer;
    transition: background-color 0.3s;
}

.modal-button:hover {
    background-color: #6e1c3b;
}
</style>
