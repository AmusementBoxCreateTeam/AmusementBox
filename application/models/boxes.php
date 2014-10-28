<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Boxes extends CI_Model {

    public function __construct() {
        parent::__construct();
        $this->load->database();
    }
    
    function get_list(){
        $this->db->select('id');
        $this->db->select('entry_date');
        $this->db->select('X(point) as x');
        $this->db->select('Y(point) as y');
        $this->db->select('delete_date');
        $this->db->from('boxes');
        
        $query = $this->db->get();
        
        return $query->result();
    }

}