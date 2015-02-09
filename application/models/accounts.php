<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Accounts extends CI_Model {

    public function __construct() {
        parent::__construct();
        $this->load->database();
    }

    function update($data) {
        $this->db->trans_start();
        $set_data = $this->create_set_data($data);
        $this->db->set($set_data);
        $this->db->where('login_id',$this->session->userdata('login_id'));
        $this->db->where($set_data);
        $this->db->update('logins');
        $this->db->trans_complete();
    }

    private function create_set_data($data) {
        $set_data = array();
        if (!empty($data['check_mail'])) {
            $set_data += array(
                'email' => $data['mail']
            );
        }
        if (!empty($data['check_pass'])) {
            $set_data += array(
                'password' => $data['pass']
            );
        }

        return $set_data;
    }

}
