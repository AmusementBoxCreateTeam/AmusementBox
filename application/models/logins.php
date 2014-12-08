<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Logins extends CI_Model {

    public function __construct() {
        parent::__construct();
        $this->load->database();
        $this->load->library('conversion_age_birthday');
    }

    public function get_login($input) {
        $this->db->select('login_id');
        $this->db->from('logins');
        $this->db->where('login_id', $input['id']);
        $this->db->where('password', $input['password']);

        $query = $this->db->get();

        if ($query->num_rows === 0) {
            return 0;
        } else {
            return $query->row();
        }
    }

    public function get_forgot($input) {
        $this->db->select('login_id');
        $this->db->select('email');
        $this->db->from('logins');
        $this->db->where('login_id', $input['id']);
        $this->db->where('email', $input['email']);

        $query = $this->db->get();

        if ($query->num_rows === 0) {
            return 0;
        } else {
            return $query->row();
        }
    }

    public function update_password($id,$password) {
        $this->db->trans_start();
        $this->db->set(array('password' => $password));
        $this->db->where('login_id', $id);
        $this->db->update('logins');
        $this->db->trans_complete();
    }

}
