<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Users extends CI_Model {

    public function __construct() {
        parent::__construct();
        $this->load->database();
    }
    
    function get_list(){
        $this->db->select('nickname');
        $this->db->select('birthday');
        $this->db->select('entry_date');
        $this->db->select('gender');
        $this->db->select('count(*) as use_count');
        $this->db->from('users');
        $this->db->join('historys','users.id','historys.user_id');
        $this->db->group_by('historys.user_id');
        $this->db->where('delete_date',null);
        
        $query = $this->db->get();
        
        return $query->result();
    }

}