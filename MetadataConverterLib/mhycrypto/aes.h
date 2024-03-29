/**
 * \file aes.h
 * \brief Header defining the API for OQS AES
 */

#ifndef __OQS_AES_H
#define __OQS_AES_H

#include <stdint.h>
#include <stdlib.h>


extern "C" void oqs_mhy128_enc_c(const uint8_t * plaintext, const void* _schedule, uint8_t * ciphertext);
extern "C" void oqs_mhy128_dec_c(const uint8_t * ciphertext, const void* _schedule, uint8_t * plaintext);
/**
 * Function to fill a key schedule given an initial key.
 *
 * @param key            Initial Key.
 * @param schedule       Abstract data structure for a key schedule.
 * @param forEncryption  1 if key schedule is for encryption, 0 if for decryption.
 */
void OQS_AES128_load_schedule(const uint8_t *key, void **schedule, int for_encryption);

/**
 * Function to free a key schedule.
 *
 * @param schedule       Schedule generated with OQS_AES128_load_schedule().
 */
void OQS_AES128_free_schedule(void *schedule);

/**
 * Function to encrypt blocks of plaintext using ECB mode.
 * A schedule based on the key is generated and used internally.
 *
 * @param plaintext     Plaintext to be encrypted.
 * @param plaintext_len Length on the plaintext in bytes. Must be a multiple of 16.
 * @param key           Key to be used for encryption.
 * @param ciphertext    Pointer to a block of memory which >= in size to the plaintext block. The result will be written here.
 */
void OQS_AES128_ECB_enc(const uint8_t *plaintext, const size_t plaintext_len, const uint8_t *key, uint8_t *ciphertext);

/**
 * Function to decrypt blocks of plaintext using ECB mode.
 * A schedule based on the key is generated and used internally.
 *
 * @param ciphertext     Ciphertext to be decrypted.
 * @param ciphertext_len Length on the ciphertext in bytes. Must be a multiple of 16.
 * @param key            Key to be used for encryption.
 * @param ciphertext     Pointer to a block of memory which >= in size to the ciphertext block. The result will be written here.
 */
void OQS_AES128_ECB_dec(const uint8_t *ciphertext, const size_t ciphertext_len, const uint8_t *key, uint8_t *plaintext);

/**
 * Same as OQS_AES128_ECB_enc() except a schedule generated by
 * OQS_AES128_load_schedule() is passed rather then a key. This is faster
 * if the same schedule is used for multiple encryptions since it does
 * not have to be regenerated from the key.
 */
void OQS_AES128_ECB_enc_sch(const uint8_t *plaintext, const size_t plaintext_len, const void *schedule, uint8_t *ciphertext);

/**
 * Same as OQS_AES128_ECB_dec() except a schedule generated by
 * OQS_AES128_load_schedule() is passed rather then a key. This is faster
 * if the same schedule is used for multiple encryptions since it does
 * not have to be regenerated from the key.
 */
void OQS_AES128_ECB_dec_sch(const uint8_t *ciphertext, const size_t ciphertext_len, const void *schedule, uint8_t *plaintext);

#endif